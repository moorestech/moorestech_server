using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Item;
using Cysharp.Threading.Tasks;
using Game.World.Interface.DataStore;
using MainGame.Network.Send;
using MainGame.Network.Settings;
using Server.Protocol;
using Server.Protocol.PacketResponse;
using Server.Util.MessagePack;
using UnityEngine;

namespace Client.Network.NewApi
{
    public class VanillaApi
    {
        private readonly ServerConnector _serverConnector;
        private readonly ItemStackFactory _itemStackFactory;
        private readonly PlayerConnectionSetting _playerConnectionSetting;

        private static VanillaApi _instance;

        public VanillaApi(ServerConnector serverConnector, ItemStackFactory itemStackFactory, PlayerConnectionSetting playerConnectionSetting)
        {
            _serverConnector = serverConnector;
            _itemStackFactory = itemStackFactory;
            _playerConnectionSetting = playerConnectionSetting;
            _instance = this;
            CollectEvent().Forget();
        }

        public static async UniTask<HandshakeResponse> InitialHandShake(int playerId,CancellationToken ct)
        {
            var request = new RequestInitialHandshakeMessagePack(playerId,$"Player {playerId}");
            var response = await _instance._serverConnector.GetInformationData<ResponseInitialHandshakeMessagePack>(request, ct);
            return new HandshakeResponse(response);
        }
        
        public static async UniTask<List<MapObjectsInfoMessagePack>> GetMapObjectInfo(CancellationToken ct)
        {
            var request = new RequestMapObjectInfosMessagePack();
            var response = await _instance._serverConnector.GetInformationData<ResponseMapObjectInfosMessagePack>(request, ct);
            return response?.MapObjects;
        }
        
        public static async UniTask<List<IItemStack>> GetBlockInventory(Vector2Int blockPos, CancellationToken ct)
        {
            var request = new RequestBlockInventoryRequestProtocolMessagePack(blockPos.x, blockPos.y);

            var response = await _instance._serverConnector.GetInformationData<BlockInventoryResponseProtocolMessagePack>(request, ct);

            var items = new List<IItemStack>(response.ItemIds.Length);
            for (int i = 0; i < response.ItemIds.Length; i++)
            {
                var id = response.ItemIds[i];
                var count = response.ItemCounts[i];
                items.Add(_instance._itemStackFactory.Create(id, count));
            }

            return items;
        }

        public static void SetOpenCloseBlock(int playerId, Vector2Int pos, bool isOpen)
        {
            var request = new BlockInventoryOpenCloseProtocolMessagePack(playerId, pos.x, pos.y, isOpen);
            _instance._serverConnector.Send(request);
        }
        
        public static async UniTask<PlayerInventoryResponse> GetPlayerInventory(int playerId, CancellationToken ct)
        {
            var request = new RequestPlayerInventoryProtocolMessagePack(playerId);

            var response = await _instance._serverConnector.GetInformationData<PlayerInventoryResponseProtocolMessagePack>(request, ct);

            var mainItems = new List<IItemStack>(response.Main.Length);
            foreach (var item in response.Main)
            {
                var id = item.Id;
                var count = item.Count;
                mainItems.Add(_instance._itemStackFactory.Create(id, count));
            }

            var grabItem = _instance._itemStackFactory.Create(response.Grab.Id, response.Grab.Count);

            return new PlayerInventoryResponse(mainItems, grabItem);
        }

        public static async UniTask<List<ChunkResponse>> GetChunkInfos(List<Vector2Int> chunks, CancellationToken ct)
        {
            var request = new RequestChunkDataMessagePack(chunks.Select(c => new Vector2IntMessagePack(c)).ToList());
            var response = await _instance._serverConnector.GetInformationData<ResponseChunkDataMessagePack>(request, ct);
            
            var result = new List<ChunkResponse>(response.ChunkData.Length);
            foreach (var responseChunk in response.ChunkData)
            {
                result.Add(ParseChunkResponse(responseChunk));
            }
            
            return result;
            
            #region Internal

            ChunkResponse ParseChunkResponse(ChunkDataMessagePack chunk)
            {
                var blocks = new BlockResponse[chunk.BlockIds.GetLength(0), chunk.BlockIds.GetLength(1)];
                for (int x = 0; x < chunk.BlockIds.GetLength(0); x++)
                {
                    for (int y = 0; y < chunk.BlockIds.GetLength(1); y++)
                    {
                        blocks[x, y] = new BlockResponse(chunk.BlockIds[x, y], (BlockDirection) chunk.BlockDirections[x, y]);
                    }
                }
                
                var entities = chunk.Entities.
                    Select(e => new EntityResponse(e));
                
                var chunkPos = chunk.ChunkPos.Vector2Int;
                return new ChunkResponse(chunkPos, blocks, entities.ToList());
            }

            #endregion
        }

        private readonly Dictionary<string,EventResponseInfo> _eventResponseInfos = new ();

        public async UniTask CollectEvent()
        {
            while (true)
            {
                var ct = new CancellationTokenSource().Token;

                try
                {
                    await RequestAndParse(ct);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Event Protocol Error:{e.Message}\n{e.StackTrace}");
                }

                await UniTask.Delay(ServerConst.PollingRateMillSec, cancellationToken: ct);
            }

            #region Internal

            async UniTask RequestAndParse(CancellationToken ct)
            {
                var request = new EventProtocolMessagePack(_playerConnectionSetting.PlayerId);
            
                var response = await _instance._serverConnector.GetInformationData<ResponseEventProtocolMessagePack>(request, ct);
            
                foreach (var eventMessagePack in response.Events)
                {
                    if (_instance._eventResponseInfos.TryGetValue(eventMessagePack.Tag, out var info))
                    {
                        info.ResponseAction(eventMessagePack.Payload);
                    }
                }
            }

            #endregion
        }
        
        public void RegisterEventResponse(string tag,Action<byte[]> responseAction)
        {
            _eventResponseInfos.Add(tag,new EventResponseInfo(responseAction,tag));
        }
        
        public void UnRegisterEventResponse(string tag)
        {
            _eventResponseInfos.Remove(tag);
        }
    }

    public class HandshakeResponse
    {
        public Vector2 PlayerPos { get; }
        
        public HandshakeResponse(ResponseInitialHandshakeMessagePack response)
        {
            PlayerPos = response.PlayerPos.Vector2;
        }
    }

    public class PlayerInventoryResponse
    {
        public List<IItemStack> MainInventory { get; }
        public IItemStack GrabItem { get; }
        
        public PlayerInventoryResponse(List<IItemStack> mainInventory, IItemStack grabItem)
        {
            MainInventory = mainInventory;
            GrabItem = grabItem;
        }
    }

    public class ChunkResponse
    {
        public readonly Vector2Int ChunkPos;
        public readonly BlockResponse[,] Blocks;
        public readonly List<EntityResponse> Entities;
        
        //TODO レスポンスの種類を増やせるようにする

        public ChunkResponse(Vector2Int chunkPos, BlockResponse[,] blocks, List<EntityResponse> entities)
        {
            ChunkPos = chunkPos;
            Blocks = blocks;
            Entities = entities;
        }
    }

    public class BlockResponse
    {
        public readonly BlockDirection BlockDirection;
        public readonly int BlockId;
        
        public BlockResponse(int blockId, BlockDirection blockDirection)
        {
            BlockId = blockId;
            BlockDirection = blockDirection;
        }
    }

    public class EntityResponse
    {
        public readonly long InstanceId;
        public readonly string Type;
        public readonly Vector3 Position;
        public readonly string State;

        public EntityResponse(EntityMessagePack entityMessagePack)
        {
            InstanceId = entityMessagePack.InstanceId;
            Type = entityMessagePack.Type;
            Position = entityMessagePack.Position.Vector3;
            State = entityMessagePack.State;
        }
    }
    
    public class EventResponseInfo
    {
        public readonly string EventTag;
        public readonly Action<byte[]> ResponseAction;

        public EventResponseInfo(Action<byte[]> responseAction, string eventTag)
        {
            ResponseAction = responseAction;
            EventTag = eventTag;
        }
    }
}