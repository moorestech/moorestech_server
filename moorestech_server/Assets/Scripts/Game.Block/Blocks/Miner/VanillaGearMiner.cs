﻿using Core.EnergySystem.Gear;
using Core.Item;
using Game.Block.Component;
using Game.Block.Event;
using Game.Block.Interface;

namespace Game.Block.Blocks.Miner
{
    public class VanillaGearMiner : VanillaMinerBase, IGearConsumer
    {
        public VanillaGearMiner(
            (int blockId, int entityId, long blockHash, int requestPower, int outputSlotCount, ItemStackFactory
                itemStackFactory, BlockOpenableInventoryUpdateEvent openableInventoryUpdateEvent, BlockPositionInfo blockPositionInfo, ComponentFactory componentFactory) data) :
            base(data.blockId, data.entityId, data.blockHash, data.requestPower, data.outputSlotCount,
                data.itemStackFactory, data.openableInventoryUpdateEvent, data.blockPositionInfo, data.componentFactory)
        {
        }

        public VanillaGearMiner(
            (string saveData, int blockId, int entityId, long blockHash, int requestPower, int outputSlotCount,
                ItemStackFactory itemStackFactory, BlockOpenableInventoryUpdateEvent openableInventoryUpdateEvent, BlockPositionInfo blockPositionInfo, ComponentFactory componentFactory) data)
            :
            base(data.saveData, data.blockId, data.entityId, data.blockHash, data.requestPower, data.outputSlotCount,
                data.itemStackFactory, data.openableInventoryUpdateEvent, data.blockPositionInfo, data.componentFactory)
        {
        }
    }
}