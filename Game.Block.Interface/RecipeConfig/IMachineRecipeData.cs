﻿using Core.Item;

namespace Game.Block.Interface.RecipeConfig
{

    public class ItemOutput
    {
        public IItemStack OutputItem { get; }

        public double Percent { get; }

        public ItemOutput(IItemStack outputItemMachine, double percent)
        {
            OutputItem = outputItemMachine;
            Percent = percent;
        }
    }
}