using System.Collections.Generic;
using System.Threading;
using Client.Game.Context;
using Client.Network.API;
using Client.Network.API;
using Game.World.Interface.DataStore;
using Constant;
using Cysharp.Threading.Tasks;
using MainGame.Presenter.Entity;
using MainGame.UnityView.Chunk;
using MessagePack;
using Server.Event.EventReceive;
using Server.Protocol.PacketResponse.Const;
using UnityEngine;
using VContainer.Unity;

namespace MainGame.Presenter.Block
{
    /// <summary>
    ///     サーバーからのパケットを受け取り、Viewにブロックの更新情報を渡す
    ///     IInitializableがないとDIコンテナ作成時にインスタンスが生成されないので実装しています
    /// </summary>
    public class ChunkDataHandler : IInitializable
    {
        private readonly ChunkBlockGameObjectDataStore _chunkBlockGameObjectDataStore;
        private readonly EntityObjectDatastore _entitiesDatastore;

        public ChunkDataHandler(ChunkBlockGameObjectDataStore chunkBlockGameObjectDataStore, EntityObjectDatastore entitiesDatastore,InitialHandshakeResponse initialHandshakeResponse)
        {
            _chunkBlockGameObjectDataStore = chunkBlockGameObjectDataStore;
            _entitiesDatastore = entitiesDatastore;
            //イベントをサブスクライブする
            MoorestechContext.VanillaApi.Event.RegisterEventResponse(PlaceBlockEventPacket.EventTag, OnBlockUpdate);

            //初期ハンドシェイクのデータを適用する
            foreach (var chunk in initialHandshakeResponse.Chunks)
            {
                ApplyChunkData(chunk);
            }
        }
        
        /// <summary>
        ///     単一のブロックの更新イベント
        /// </summary>
        private void OnBlockUpdate(byte[] payload)
        {
            var data = MessagePackSerializer.Deserialize<PlaceBlockEventMessagePack>(payload);
            
            var blockPos = (Vector3Int)data.BlockData.BlockPos;
            var blockId = data.BlockData.BlockId;
            var blockDirection = data.BlockData.BlockDirection;

            //viewにブロックがおかれたことを通知する
            PlaceOrRemoveBlock(blockPos, blockId, blockDirection);
        }

        public void Initialize()
        {
            OnChunkUpdate().Forget();
        }

        /// <summary>
        /// 0.5秒に1回のチャンクの更新イベント
        /// </summary>
        private async UniTask OnChunkUpdate()
        {
            var ct = new CancellationTokenSource().Token;
            
            while (true)
            {
                await GetChunkAndApply();
                await UniTask.Delay(500, cancellationToken: ct); //TODO 本当に0.5秒に1回でいいのか？
            }

            #region Internal

            async UniTask GetChunkAndApply()
            {
                var data = await MoorestechContext.VanillaApi.Response.GetChunkInfos(ct);
                if (data == null)
                {
                    return;
                }
                
                foreach (var chunk in data)
                {
                    ApplyChunkData(chunk);
                }
            }


            #endregion
        }
        
        
        private void ApplyChunkData(ChunkResponse chunk)
        {
            foreach (var block in chunk.Blocks)
            {
                PlaceOrRemoveBlock(block.BlockPos, block.BlockId, block.BlockDirection);
            }

            if (chunk.Entities == null)
            {
                Debug.Log("chunk.Entities is null");
                return;
            }
            _entitiesDatastore.OnEntitiesUpdate(chunk.Entities);
        }


        private void PlaceOrRemoveBlock(Vector3Int position, int id, BlockDirection blockDirection)
        {
            if (id == BlockConstant.NullBlockId)
            {
                _chunkBlockGameObjectDataStore.GameObjectBlockRemove(position);
                return;
            }

            _chunkBlockGameObjectDataStore.GameObjectBlockPlace(position, id, blockDirection);
        }


        /// <summary>
        ///     ブロックの座標とチャンクの座標から、IDの配列のインデックスを取得する
        /// </summary>
        private Vector2Int GetBlockArrayIndex(Vector2Int chunkPos, Vector2Int blockPos)
        {
            var (x, y) = (
                GetBlockArrayIndex(chunkPos.x, blockPos.x),
                GetBlockArrayIndex(chunkPos.y, blockPos.y));
            return new Vector2Int(x, y);
        }

        private int GetBlockArrayIndex(int chunkPos, int blockPos)
        {
            if (0 <= chunkPos) return blockPos - chunkPos;
            return -chunkPos - -blockPos;
        }
    }
    
    

}