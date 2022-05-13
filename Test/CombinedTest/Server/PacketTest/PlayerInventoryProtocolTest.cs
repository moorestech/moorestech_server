using System;
using System.Collections.Generic;
using System.Linq;
using Core.ConfigJson;
using Core.Const;
using Core.Item;
using Core.Item.Config;
using Game.Crafting.Interface;
using Game.PlayerInventory.Interface;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server;
using Server.StartServerSystem;
using Server.Util;
using Test.Module.TestConfig;
using Test.Module.TestMod;

namespace Test.CombinedTest.Server.PacketTest
{
    public class PlayerInventoryProtocolTest
    {
        [Test]
        public void GetPlayerInventoryProtocolTest()
        {
            int playerId = 1;

            var (packet, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModDirectory.ForUnitTestModDirectory);


            //からの時のデータ要求
            var payload = new List<byte>();
            payload.AddRange(ToByteList.Convert((short) 3));
            payload.AddRange(ToByteList.Convert(playerId));

            var response = new ByteListEnumerator(packet.GetPacketResponse(payload)[0].ToList());

            //データの検証
            Assert.AreEqual(4, response.MoveNextToGetShort());
            Assert.AreEqual(playerId, response.MoveNextToGetInt());
            Assert.AreEqual(0, response.MoveNextToGetShort());
            
            //プレイヤーインベントリの検証
            for (int i = 0; i < PlayerInventoryConst.MainInventoryColumns; i++)
            {
                Assert.AreEqual(ItemConst.EmptyItemId, response.MoveNextToGetInt());
                Assert.AreEqual(0, response.MoveNextToGetInt());
            }
            
            //グラブインベントリの検証
            Assert.AreEqual(0, response.MoveNextToGetInt());
            Assert.AreEqual(0, response.MoveNextToGetInt());
            
            //クラフトインベントリの検証
            for (int i = 0; i < PlayerInventoryConst.CraftingSlotSize; i++)
            {
                Assert.AreEqual(ItemConst.EmptyItemId, response.MoveNextToGetInt());
                Assert.AreEqual(0, response.MoveNextToGetInt());
            }
            //クラフト結果アイテムの検証
            Assert.AreEqual(ItemConst.EmptyItemId, response.MoveNextToGetInt());
            Assert.AreEqual(0, response.MoveNextToGetInt());
            //クラフト不可能である事の検証
            Assert.AreEqual(0, response.MoveNextToGetByte());
            
            
            
            //インベントリにアイテムが入っている時のテスト
            var playerInventoryData = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(playerId);
            var itemStackFactory = serviceProvider.GetService<ItemStackFactory>();
            playerInventoryData.MainOpenableInventory.SetItem(0, itemStackFactory.Create(1, 5));
            playerInventoryData.MainOpenableInventory.SetItem(20, itemStackFactory.Create(3, 1));
            playerInventoryData.MainOpenableInventory.SetItem(34, itemStackFactory.Create(10, 7));
            
            
            
            //クラフトに必要なアイテムの二倍の量を入れる
            var craftConfig = serviceProvider.GetService<ICraftingConfig>().GetCraftingConfigList()[0];
            for (int i = 0; i < craftConfig.Items.Count; i++)
            {
                var id = craftConfig.Items[i].Id;
                var count = craftConfig.Items[i].Count;
                Console.WriteLine(craftConfig.Items[i].Id);
                Console.WriteLine(craftConfig.Items[i].Count);
                playerInventoryData.CraftingOpenableInventory.SetItem(i,id,count * 2);
            }
            packet.GetPacketResponse(payload);
            
            //クラフトを実行する　ここでアイテムが消費される
            playerInventoryData.CraftingOpenableInventory.NormalCraft();

            
            
            //2回目のデータ要求
            response = new ByteListEnumerator(packet.GetPacketResponse(payload)[0].ToList());
            Assert.AreEqual(4, response.MoveNextToGetShort());
            Assert.AreEqual(playerId, response.MoveNextToGetInt());
            Assert.AreEqual(0, response.MoveNextToGetShort());

            //データの検証
            for (int i = 0; i < PlayerInventoryConst.MainInventorySize; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(1, response.MoveNextToGetInt());
                    Assert.AreEqual(5, response.MoveNextToGetInt());
                }
                else if (i == 20)
                {
                    Assert.AreEqual(3, response.MoveNextToGetInt());
                    Assert.AreEqual(1, response.MoveNextToGetInt());
                }
                else if (i == 34)
                {
                    Assert.AreEqual(10, response.MoveNextToGetInt());
                    Assert.AreEqual(7, response.MoveNextToGetInt());
                }
                else
                {
                    Assert.AreEqual(ItemConst.EmptyItemId, response.MoveNextToGetInt());
                    Assert.AreEqual(0, response.MoveNextToGetInt());
                }
            }
            
            //グラブインベントリの検証
            //クラフトしたのでグラブインベントリに入っている
            Assert.AreEqual(craftConfig.Result.Id, response.MoveNextToGetInt());
            Assert.AreEqual(craftConfig.Result.Count, response.MoveNextToGetInt());
            
            
            //クラフトスロットの検証
            for (int i = 0; i < PlayerInventoryConst.CraftingSlotSize; i++)
            {
                Assert.AreEqual(craftConfig.Items[i].Id, response.MoveNextToGetInt());
                Assert.AreEqual(craftConfig.Items[i].Count, response.MoveNextToGetInt());
            }

            //クラフト結果アイテムの検証
            Assert.AreEqual(craftConfig.Result.Id, response.MoveNextToGetInt());
            Assert.AreEqual(craftConfig.Result.Count, response.MoveNextToGetInt());
            //まだクラフトスロットにアイテムがあるため、クラフト可能である事の検証
            response.MoveNextToGetByte();
            Assert.AreEqual(1, 1);
        }
    }
}