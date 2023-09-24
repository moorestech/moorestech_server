﻿using System.Collections.Generic;
using System.Linq;
using Core.Const;
using Core.Item;

namespace Game.Block.Interface.RecipeConfig
{
    public class MachineRecipeData
    {
        public int BlockId { get; }

        public MachineRecipeData(int blockId, int time, List<IItemStack> itemInputs, List<ItemOutput> itemOutputs,
            int recipeId)
        {
            BlockId = blockId;
            ItemInputs = itemInputs;
            ItemOutputs = itemOutputs;
            RecipeId = recipeId;
            Time = time;
        }

        public static MachineRecipeData CreateEmptyRecipe()
        {
            return new MachineRecipeData(BlockConst.EmptyBlockId, 0, new List<IItemStack>(), new List<ItemOutput>(), -1);
        }

        public List<IItemStack> ItemInputs { get; }

        public List<ItemOutput> ItemOutputs { get; }

        public int Time { get; }
        public int RecipeId { get; }

        public bool RecipeConfirmation(IReadOnlyList<IItemStack> inputSlot,int blockId)
        {
            if (blockId != BlockId) return false;
            
            int cnt = 0;
            foreach (var slot in inputSlot)
            {
                cnt += ItemInputs.Count(input => slot.Id == input.Id && input.Count <= slot.Count);
            }

            return cnt == ItemInputs.Count;
        }
    }
}