using System.Collections.Generic;
using System.Linq;
using Game.Block.Blocks;
using Game.Block.Interface;
using Game.World.Interface.DataStore;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server.Boot;
using Server.Event.EventReceive;
using Server.Protocol.PacketResponse;
using Tests.Module.TestMod;
using UnityEngine;
using Random = System.Random;

namespace Tests.CombinedTest.Server.PacketTest.Event
{
    public class BlockPlaceEventPacketTest
    {
        //ブロックを設置しなかった時何も返ってこないテスト
        [Test]
        public void DontBlockPlaceTest()
        {
            var (packetResponse, _) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            
            var response = packetResponse.GetPacketResponse(EventRequestData(0));
            var eventMessagePack = MessagePackSerializer.Deserialize<ResponseEventProtocolMessagePack>(response[0].ToArray());
            Assert.AreEqual(0, eventMessagePack.Events.Count);
        }

        //ブロックを0個以上設置した時にブロック設置イベントが返ってくるテスト
        [Test]
        public void BlockPlaceEvent()
        {
            var (packetResponse, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDataStore = serviceProvider.GetService<IWorldBlockDatastore>();


            //イベントキューにIDを登録する
            var response = packetResponse.GetPacketResponse(EventRequestData(0));
            var eventMessagePack = MessagePackSerializer.Deserialize<ResponseEventProtocolMessagePack>(response[0].ToArray());
            Assert.AreEqual(0, eventMessagePack.Events.Count);

            var random = new Random(1410);
            
            //ランダムな位置にブロックを設置する
            var blocks = new List<TestBlockData>();
            for (var j = 0; j < 10; j++)
            {
                var x = random.Next(-10000, 10000);
                var y = random.Next(-10000, 10000);
                var pos = new Vector3Int(x, y);
                var blockId = random.Next(1, 1000);
                var direction = random.Next(0, 4);

                //設置したブロックを保持する
                blocks.Add(new TestBlockData(pos, blockId, direction));
                //ブロックの設置
                var blockPosInfo = new BlockPositionInfo(pos, (BlockDirection)direction,Vector3Int.one);
                worldBlockDataStore.AddBlock(new VanillaBlock(blockId, random.Next(1, 1000000), 1,blockPosInfo));
            }


            //イベントパケットをリクエストする
            response = packetResponse.GetPacketResponse(EventRequestData(0));
            eventMessagePack = MessagePackSerializer.Deserialize<ResponseEventProtocolMessagePack>(response[0].ToArray());

            //返ってきたイベントパケットと設置したブロックを照合し、あったら削除する
            foreach (var r in eventMessagePack.Events)
            {
                var b = AnalysisResponsePacket(r.Payload);
                for (var j = 0; j < blocks.Count; j++)
                    if (b.Equals(blocks[j]))
                        blocks.RemoveAt(j);
            }

            //設置したブロックリストが残ってなければすべてのイベントが返ってきた事がわかる
            Assert.AreEqual(0, blocks.Count);


            //イベントのリクエストを送ったので次は何も返ってこないテスト
            response = packetResponse.GetPacketResponse(EventRequestData(0));
            eventMessagePack = MessagePackSerializer.Deserialize<ResponseEventProtocolMessagePack>(response[0].ToArray());
            Assert.AreEqual(0, eventMessagePack.Events.Count);
        }

        private TestBlockData AnalysisResponsePacket(byte[] payload)
        {
            var data = MessagePackSerializer.Deserialize<PlaceBlockEventMessagePack>(payload).BlockData;

            return new TestBlockData(data.BlockPos, data.BlockId, data.Direction);
        }

        private List<byte> EventRequestData(int plyaerID)
        {
            return MessagePackSerializer.Serialize(new EventProtocolMessagePack(plyaerID)).ToList();
        }

        private class TestBlockData
        {
            public readonly BlockDirection BlockDirection;
            public readonly int id;
            public readonly int X;
            public readonly int Y;

            public TestBlockData(Vector3Int pos, int id, int blockDirectionNum)
            {
                X = pos.x;
                Y = pos.y;
                this.id = id;
                BlockDirection = (BlockDirection)blockDirectionNum;
            }

            public override bool Equals(object? obj)
            {
                var b = obj as TestBlockData;
                return
                    b.id == id &&
                    b.X == X &&
                    b.Y == Y &&
                    b.BlockDirection == BlockDirection;
            }
        }
    }
}