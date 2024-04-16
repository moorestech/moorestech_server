using System;
using System.Collections.Generic;
using System.Linq;
using Game.Block.Interface;
using Game.Block.Interface.Component;
using Game.Block.Interface.ComponentAttribute;
using Game.Context;
using Game.World.Interface;
using Game.World.Interface.DataStore;
using UnityEngine;

namespace Game.Block.Component.IOConnector
{
    [DisallowMultiple]
    public class BlockConnectorComponent<TTarget> : IBlockComponent where TTarget : IBlockComponent
    {
        private readonly BlockDirection _blockDirection;
        private readonly Vector3Int _blockPos;

        private readonly List<IDisposable> _blockUpdateEvents = new();
        private readonly List<TTarget> _connectTargets = new();
        private readonly IOConnectionSetting _ioConnectionSetting;

        public BlockConnectorComponent(IOConnectionSetting ioConnectionSetting, BlockPositionInfo blockPositionInfo)
        {
            _blockPos = blockPositionInfo.OriginalPos;
            _blockDirection = blockPositionInfo.BlockDirection;
            _ioConnectionSetting = ioConnectionSetting;

            if (_ioConnectionSetting.OutputConnector != null)
            {
                var worldBlockUpdateEvent = ServerContext.WorldBlockUpdateEvent;
                List<Vector3Int> outputPoss = ConvertConnectDirection(_ioConnectionSetting.OutputConnector);
                foreach (var outputPos in outputPoss)
                {
                    _blockUpdateEvents.Add(worldBlockUpdateEvent.SubscribePlace(outputPos, b => PlaceBlock(b.Pos)));
                    _blockUpdateEvents.Add(worldBlockUpdateEvent.SubscribeRemove(outputPos, RemoveBlock));

                    //アウトプット先にブロックがあったら接続を試みる
                    if (ServerContext.WorldBlockDatastore.Exists(outputPos))
                    {
                        PlaceBlock(outputPos);
                    }
                }
            }

            #region Internal

            // 接続先のブロックの接続可能な位置を取得する
            List<Vector3Int> ConvertConnectDirection(ConnectDirection[] connectDirection)
            {
                var blockPosConvertAction = _blockDirection.GetCoordinateConvertAction();

                IEnumerable<Vector3Int> convertedPositions = connectDirection.Select(c => blockPosConvertAction(c.ToVector3Int()) + _blockPos);
                return convertedPositions.ToList();
            }

            #endregion
        }
        public IReadOnlyList<TTarget> ConnectTargets => _connectTargets;

        public bool IsDestroy { get; private set; }

        public void Destroy()
        {
            _connectTargets.Clear();
            _blockUpdateEvents.ForEach(x => x.Dispose());
            _blockUpdateEvents.Clear();
            IsDestroy = true;
        }

        /// <summary>
        ///     ブロックを接続元から接続先に接続できるなら接続する
        ///     その場所にブロックがあるか、
        ///     それぞれインプットとアウトプットの向きはあっているかを確認し、接続する
        /// </summary>
        private void PlaceBlock(Vector3Int destinationPos)
        {
            //接続先にBlockInventoryがなければ処理を終了
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            if (!worldBlockDatastore.TryGetBlock<BlockConnectorComponent<TTarget>>(destinationPos, out var destinationInputConnector)) return;
            if (!worldBlockDatastore.TryGetBlock<TTarget>(destinationPos, out var targetComponent)) return;

            //接続元のブロックデータを取得
            (_, List<ConnectDirection> sourceBlockOutputConnector) = GetConnectionPositions(_ioConnectionSetting, _blockDirection);


            //接続先のブロックデータを取得
            var blockId = worldBlockDatastore.GetBlock(destinationPos).BlockId;
            var destinationBlockType = ServerContext.BlockConfig.GetBlockConfig(blockId).Type;

            var destinationSetting = destinationInputConnector._ioConnectionSetting;
            var destinationDirection = worldBlockDatastore.GetBlockDirection(destinationPos);
            (List<ConnectDirection> destinationBlockInputConnector, _) = GetConnectionPositions(destinationSetting, destinationDirection);

            //接続元の接続可能リストに接続先がなかったら終了
            if (!_ioConnectionSetting.ConnectableBlockType.Contains(destinationBlockType)) return;


            //接続元から接続先へのブロックの距離を取得
            var distance = destinationPos - _blockPos;

            //接続元ブロックに対応するアウトプット座標があるかチェック
            var source = new ConnectDirection(distance);
            if (!sourceBlockOutputConnector.Contains(source)) return;

            //接続先ブロックに対応するインプット座標があるかチェック
            var destination = new ConnectDirection(distance * -1);
            if (!destinationBlockInputConnector.Contains(destination)) return;


            //接続元ブロックと接続先ブロックを接続
            if (!_connectTargets.Contains(targetComponent))
            {
                _connectTargets.Add(targetComponent);
            }

            #region Internal

            // 接続先のブロックの接続可能な位置を取得する
            (List<ConnectDirection> input, List<ConnectDirection> output) GetConnectionPositions(IOConnectionSetting connectionSetting, BlockDirection blockDirection)
            {
                ConnectDirection[] rawInputConnector = connectionSetting.InputConnector;
                ConnectDirection[] rawOutputConnector = connectionSetting.OutputConnector;

                var blockPosConvertAction = blockDirection.GetCoordinateConvertAction();

                var inputPoss = rawInputConnector.Select(ConvertConnectDirection).ToList();
                var outputPoss = rawOutputConnector.Select(ConvertConnectDirection).ToList();

                return (inputPoss, outputPoss);

                ConnectDirection ConvertConnectDirection(ConnectDirection connectDirection)
                {
                    var convertedVector = blockPosConvertAction(connectDirection.ToVector3Int());
                    return new ConnectDirection(convertedVector);
                }
            }

            #endregion
        }

        private void RemoveBlock(BlockUpdateProperties updateProperties)
        {
            //削除されたブロックがInputConnectorComponentでない場合、処理を終了する
            if (!ServerContext.WorldBlockDatastore.TryGetBlock<TTarget>(updateProperties.Pos, out var component)) return;

            _connectTargets.Remove(component);
        }
    }
}