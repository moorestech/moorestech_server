using Core.Master;
using Game.Block.Interface;
using Game.Context;
using NUnit.Framework;
using Server.Boot;
using Tests.Module.TestMod;
using UnityEngine;
using static Tests.Module.TestMod.ForUnitTestModBlockId;

namespace Tests.CombinedTest.Game
{
    public class CraftChainerTest
    {
        [Test]
        public void SimpleChainerTest()
        {
            var (_, saveServiceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            
            
            
        }
        
        private CraftChainerTestNetworkContainer CreateNetwork()
        {
            //TODO イメージ図

            // クラフトチェイナーの部分
            var mainComputer = AddBlock(CraftChainerMainComputer, 0, 0, BlockDirection.North);
            AddBlock(CraftChainerTransporter, 1, 0, BlockDirection.East);
            AddBlock(CraftChainerTransporter, 0, 1, BlockDirection.South);
            AddBlock(CraftChainerTransporter, 1, 1, BlockDirection.East);
            AddBlock(CraftChainerTransporter, 2, 1, BlockDirection.East);
            AddBlock(CraftChainerTransporter, 3, 1, BlockDirection.East);
            var providerChest = AddBlock(CraftChainerProviderChest, 0, 2, BlockDirection.North);
            var crafter1 = AddBlock(CraftChainerCrafter, 2, 2, BlockDirection.North);
            var crafter2 = AddBlock(CraftChainerCrafter, 3, 2, BlockDirection.North);
            
            // クラフトチェイナーの部分
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
            
            return new CraftChainerTestNetworkContainer(mainComputer, crafter1, crafter2, providerChest);
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
        }
    }
    
}