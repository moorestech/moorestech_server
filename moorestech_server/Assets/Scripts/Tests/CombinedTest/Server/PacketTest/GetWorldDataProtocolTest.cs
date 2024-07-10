using System.Collections.Generic;
using System.Linq;
using Core.Const;
using Game.Block.Interface;
using Game.Context;
using MessagePack;
using NUnit.Framework;
using Server.Boot;
using Server.Protocol.PacketResponse;
using Tests.Module.TestMod;
using UnityEngine;
using Random = System.Random;

namespace Tests.CombinedTest.Server.PacketTest
{
    public class GetWorldDataProtocolTest
    {
        public const int Block_1x4_Id = 9; // 1x4サイズのブロックのID
        
        //ランダムにブロックを設置するテスト
        [Test]
        public void RandomPlaceBlockToWorldDataResponseTest()
        {
            var (packetResponse, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            
            var random = new Random(13944156);
            //ブロックの設置
            for (var i = 0; i < 1000; i++)
            {
                var blockDirection = (BlockDirection)random.Next(0, 4);
                var pos = new Vector3Int(random.Next(-40, 40), random.Next(-40, 40));
                
                var blockId = random.Next(0, 3) == 1
                    ? random.Next(short.MaxValue, int.MaxValue)
                    : random.Next(1, 500);
                worldBlockDatastore.TryAddBlock(blockId, pos, blockDirection, out _);
            }
            
            var requestBytes = MessagePackSerializer.Serialize(new RequestWorldDataMessagePack());
            List<byte> responseBytes = packetResponse.GetPacketResponse(requestBytes.ToList())[0];
            var responseWorld = MessagePackSerializer.Deserialize<ResponseWorldDataMessagePack>(responseBytes.ToArray());
            
            //検証
            for (var i = 0; i < responseWorld.Blocks.Length; i++)
            {
                var block = responseWorld.Blocks[i];
                var pos = block.BlockPos;
                
                var id = worldBlockDatastore.GetOriginPosBlock(pos)?.Block.BlockId ?? BlockConst.EmptyBlockId;
                Assert.AreEqual(id, block.BlockId);
                
                var direction = worldBlockDatastore.GetOriginPosBlock(pos)?.BlockPositionInfo.BlockDirection ?? BlockDirection.North;
                Assert.AreEqual(direction, block.BlockDirection);
            }
        }
        
        //マルチブロックを設置するテスト
        [Test]
        public void PlaceBlockToWorldDataTest()
        {
            var (packetResponse, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlock = ServerContext.WorldBlockDatastore;
            
            //ブロックの設置
            worldBlock.TryAddBlock(Block_1x4_Id, Vector3Int.zero, BlockDirection.North, out _);
            
            var requestBytes = MessagePackSerializer.Serialize(new RequestWorldDataMessagePack());
            List<byte> responseBytes = packetResponse.GetPacketResponse(requestBytes.ToList())[0];
            var responseWorld = MessagePackSerializer.Deserialize<ResponseWorldDataMessagePack>(responseBytes.ToArray());
            
            //ブロックが設置されていることを確認する
            Assert.AreEqual(Block_1x4_Id, responseWorld.Blocks[0].BlockId);
        }
    }
}