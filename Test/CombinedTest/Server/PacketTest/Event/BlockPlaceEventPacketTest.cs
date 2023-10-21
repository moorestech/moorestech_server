#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Block.Blocks;
using Game.World.Interface.DataStore;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server.Boot;
using Server.Event;
using Server.Event.EventReceive;
using Server.Protocol.PacketResponse;
using Test.Module.TestMod;
using World.Event;

namespace Test.CombinedTest.Server.PacketTest.Event
{
    public class BlockPlaceEventPacketTest
    {
        
        [Test]
        public void DontBlockPlaceTest()
        {
            var blockPlace = new BlockPlaceEvent();

            var eventProtocol = new EventProtocolProvider();
            var (packetResponse, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModDirectory.ForUnitTestModDirectory);
            var response = packetResponse.GetPacketResponse(EventRequestData(0));
            Assert.AreEqual(response.Count, 0);
        }

        //0
        [Test]
        public void BlockPlaceEvent()
        {
            var (packetResponse, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDataStore = serviceProvider.GetService<IWorldBlockDatastore>();


            //ID
            var response = packetResponse.GetPacketResponse(EventRequestData(0));
            Assert.AreEqual(0, response.Count);

            var random = new Random(1410);
            for (var i = 0; i < 100; i++)
            {
                
                Console.WriteLine(i);
                var blocks = new List<TestBlockData>();
                var cnt = random.Next(0, 20);
                for (var j = 0; j < cnt; j++)
                {
                    var x = random.Next(-10000, 10000);
                    var y = random.Next(-10000, 10000);
                    var blockId = random.Next(1, 1000);
                    var direction = random.Next(0, 4);

                    
                    blocks.Add(new TestBlockData(x, y, blockId, direction));
                    
                    worldBlockDataStore.AddBlock(new VanillaBlock(blockId, random.Next(1, 1000000), 1), x, y, (BlockDirection)direction);
                }


                
                response = packetResponse.GetPacketResponse(EventRequestData(0));

                Assert.AreEqual(cnt, response.Count);


                
                foreach (var r in response)
                {
                    var b = AnalysisResponsePacket(r);
                    for (var j = 0; j < blocks.Count; j++)
                        if (b.Equals(blocks[j]))
                            blocks.RemoveAt(j);
                }

                
                Assert.AreEqual(0, blocks.Count);


                
                response = packetResponse.GetPacketResponse(EventRequestData(0));
                Assert.AreEqual(0, response.Count);
            }
        }

        private TestBlockData AnalysisResponsePacket(List<byte> payload)
        {
            var data = MessagePackSerializer.Deserialize<PlaceBlockEventMessagePack>(payload.ToArray());

            Assert.AreEqual(PlaceBlockToSetEventPacket.EventTag, data.EventTag);

            return new TestBlockData(data.X, data.Y, data.BlockId, data.Direction);
        }

        private List<byte> EventRequestData(int plyaerID)
        {
            return MessagePackSerializer.Serialize(new EventProtocolMessagePack(plyaerID)).ToList();
            ;
        }

        private class TestBlockData
        {
            public readonly BlockDirection BlockDirection;
            public readonly int id;
            public readonly int X;
            public readonly int Y;

            public TestBlockData(int x, int y, int id, int blockDirectionNum)
            {
                X = x;
                Y = y;
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
#endif