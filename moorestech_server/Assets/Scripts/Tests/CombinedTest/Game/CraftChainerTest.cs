using System;
using System.Collections.Generic;
using Core.Item.Interface;
using Core.Master;
using Game.Block.Blocks.Chest;
using Game.Block.Interface;
using Game.Context;
using Game.CraftChainer.BlockComponent.Computer;
using Game.CraftChainer.BlockComponent.Crafter;
using Game.CraftChainer.CraftChain;
using NUnit.Framework;
using Server.Boot;
using Tests.Module.TestMod;
using UnityEngine;
using static Tests.Module.TestMod.ForUnitTestModBlockId;

namespace Tests.CombinedTest.Game
{
    public class CraftChainerTest
    {
        public ItemId ItemAId;
        public ItemId ItemBId;
        public ItemId ItemCId;
        
        [SetUp]
        public void SetupCraftChainerTest()
        {
            var (_, saveServiceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            
            Guid itemAGuid = new("189672cb-6811-4080-bde1-1f9ff0ec63ff");
            Guid itemBGuid = new("547791fe-bfd8-4748-aafa-c7449391eca5");
            Guid itemCGuid = new("c8d16ba4-8a7d-4ab1-80a4-5a9c0a119627");
            ItemAId = MasterHolder.ItemMaster.GetItemId(itemAGuid);
            ItemBId = MasterHolder.ItemMaster.GetItemId(itemBGuid);
            ItemCId = MasterHolder.ItemMaster.GetItemId(itemCGuid);
        }
        
        [Test]
        public void SimpleChainerTest()
        {
            var (_, saveServiceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            
            // ネットワークの作成
            // Create a network
            var network = CreateNetwork();
            
            // 供給チェストにアイテム設定
            // Set items in the provider chest
            var chestItems = new List<IItemStack>()
            {
                ServerContext.ItemStackFactory.Create(ItemCId, 5),
            };
            network.SetProviderChestItem(chestItems);
            
            // メインコンピュータにアイテム作成リクエスト
            // Item creation request to the main computer
            var success = network.SetRequestMainComputer(ItemAId, 1);
            Assert.IsTrue(success);
            
            // 20秒たってもクラフトされない場合は失敗
            // Fail if not crafted after 20 seconds
            var now = DateTime.Now;
            
            while (true)
            {
                if (network.OnMainComputerItemExist(ItemAId, 1))
                {
                    Assert.Pass();
                    break;
                }
                
                if (DateTime.Now - now > TimeSpan.FromSeconds(20))
                {
                    Assert.Fail("Failed to create item");
                }
            }
        }
        
        private CraftChainerTestNetworkContainer CreateNetwork()
        {
            //TODO イメージ図

            // クラフトチェイナーの部分
            // Parts of the craft chainer
            var mainComputer = AddBlock(CraftChainerMainComputer, 0, 0, BlockDirection.North);
            AddBlock(CraftChainerTransporter, 1, 0, BlockDirection.East);
            AddBlock(CraftChainerTransporter, 0, 1, BlockDirection.South);
            AddBlock(CraftChainerTransporter, 1, 1, BlockDirection.East);
            AddBlock(CraftChainerTransporter, 2, 1, BlockDirection.East);
            AddBlock(CraftChainerTransporter, 3, 1, BlockDirection.East);
            var providerChest = AddBlock(CraftChainerProviderChest, 0, 2, BlockDirection.North);
            var crafter1 = AddBlock(CraftChainerCrafter, 2, 2, BlockDirection.North);
            var crafter2 = AddBlock(CraftChainerCrafter, 3, 2, BlockDirection.North);
            
            // 工場の部分
            // Parts of the factory
            AddBlock(CraftChainerBeltConveyor, 2, 3, BlockDirection.North);
            AddBlock(CraftChainerBeltConveyor, 3, 3, BlockDirection.North);
            AddBlock(CraftChainerMachine, 2, 4, BlockDirection.North);
            AddBlock(CraftChainerMachine, 3, 4, BlockDirection.North);
            
            AddBlock(CraftChainerBeltConveyor, 0, 5, BlockDirection.South);
            AddBlock(CraftChainerBeltConveyor, 1, 5, BlockDirection.West);
            AddBlock(CraftChainerBeltConveyor, 2, 5, BlockDirection.West);
            AddBlock(CraftChainerBeltConveyor, 3, 5, BlockDirection.West);
            
            AddBlock(CraftChainerBeltConveyor, 0, 4, BlockDirection.South);
            AddBlock(CraftChainerBeltConveyor, 0, 3, BlockDirection.South);
            
            var container = new CraftChainerTestNetworkContainer(mainComputer, crafter1, crafter2, providerChest);
            
            // レシピの設定
            // Recipe setting
            var inputItem1 = new List<CraftingSolverItem>
            {
                new(ItemCId, 2),
            };
            var outputItem1 = new List<CraftingSolverItem>
            {
                new(ItemBId, 1),
            };
            container.SetCrafter1Recipe(inputItem1, outputItem1);
            
            var inputItem2 = new List<CraftingSolverItem>
            {
                new(ItemBId, 2),
                new(ItemCId, 1),
            };
            var outputItem2 = new List<CraftingSolverItem>
            {
                new(ItemAId, 1),
            };
            container.SetCrafter2Recipe(inputItem2, outputItem2);
            
            
            return container;
        }
        
        private IBlock AddBlock(BlockId blockId, int x, int z, BlockDirection direction)
        {
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            worldBlockDatastore.TryAddBlock(blockId, new Vector3Int(x, 0, z), direction, out var block);
            
            return block;
        }
        
        public class CraftChainerTestNetworkContainer
        {
            public readonly IBlock MainComputer;
            public readonly IBlock Crafter1;
            public readonly IBlock Crafter2;
            public readonly IBlock ProviderChest;
            public CraftChainerTestNetworkContainer(IBlock mainComputer, IBlock crafter1, IBlock crafter2, IBlock providerChest)
            {
                MainComputer = mainComputer;
                Crafter1 = crafter1;
                Crafter2 = crafter2;
                ProviderChest = providerChest;
            }
            
            public void SetCrafter1Recipe(List<CraftingSolverItem> inputItems, List<CraftingSolverItem> outputItem)
            {
                SetCrafterRecipe(Crafter1, inputItems, outputItem);
            }
            public void SetCrafter2Recipe(List<CraftingSolverItem> inputItems, List<CraftingSolverItem> outputItem)
            {
                SetCrafterRecipe(Crafter2, inputItems, outputItem);
            }
            
            private void SetCrafterRecipe(IBlock crafter, List<CraftingSolverItem> inputItems, List<CraftingSolverItem> outputItem)
            {
                var crafterComponent = crafter.ComponentManager.GetComponent<ChainerCrafterComponent>();
                crafterComponent.SetRecipe(inputItems, outputItem);
            }
            
            public void SetProviderChestItem(List<IItemStack> items)
            {
                var chestComponent = ProviderChest.ComponentManager.GetComponent<VanillaChestComponent>();
                chestComponent.InsertItem(items);
            }
            
            public bool SetRequestMainComputer(ItemId item, int count)
            {
                var mainComputerComponent = MainComputer.ComponentManager.GetComponent<ChainerMainComputerComponent>();
                return mainComputerComponent.StartCreateItem(item, count);
            }
            
            public bool OnMainComputerItemExist(ItemId targetItem, int count)
            {
                var chest = MainComputer.ComponentManager.GetComponent<VanillaChestComponent>();
                
                var existCount = 0;
                foreach (var item in chest.InventoryItems)
                {
                    if (item.Id == targetItem)
                    {
                        existCount += item.Count;
                    }
                }
                
                return existCount >= count;
            }
        }
    }
    
}