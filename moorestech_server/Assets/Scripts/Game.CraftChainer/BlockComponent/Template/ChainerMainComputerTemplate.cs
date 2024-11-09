using System.Collections.Generic;
using Game.Block.Blocks;
using Game.Block.Blocks.Chest;
using Game.Block.Factory.BlockTemplate;
using Game.Block.Interface;
using Game.Block.Interface.Component;
using Game.CraftChainer.BlockComponent.Computer;
using Mooresmaster.Model.BlocksModule;

namespace Game.CraftChainer.BlockComponent.Template
{
    public class ChainerMainComputerTemplate : IBlockTemplate
    {
        public IBlock New(BlockMasterElement blockMasterElement, BlockInstanceId blockInstanceId, BlockPositionInfo blockPositionInfo)
        {
            return GetBlock(null, blockMasterElement, blockInstanceId, blockPositionInfo);
        }
        
        public IBlock Load(Dictionary<string, string> componentStates, BlockMasterElement blockMasterElement, BlockInstanceId blockInstanceId, BlockPositionInfo blockPositionInfo)
        {
            return GetBlock(componentStates, blockMasterElement, blockInstanceId, blockPositionInfo);
        }
        
        private BlockSystem GetBlock(Dictionary<string, string> componentStates, BlockMasterElement blockMasterElement, BlockInstanceId blockInstanceId, BlockPositionInfo blockPositionInfo)
        {
            var chest = blockMasterElement.BlockParam as CraftChainerMainComputerBlockParam;
            var inputConnectorComponent = BlockTemplateUtil.CreateInventoryConnector(chest.InventoryConnectors, blockPositionInfo);
            var inserter = new ChainerMainComputerInserter();
            
            var chestComponent = componentStates == null ?
                new VanillaChestComponent(blockInstanceId, chest.ItemSlotCount, inserter) :
                new VanillaChestComponent(componentStates, blockInstanceId, chest.ItemSlotCount, inserter);
            
            var mainComputerComponent = new ChainerMainComputerComponent();
            
            
            var components = new List<IBlockComponent>
            {
                chestComponent,
                inputConnectorComponent,
                mainComputerComponent
            };
            
            return new BlockSystem(blockInstanceId, blockMasterElement.BlockGuid, components, blockPositionInfo);
        }
    }
}