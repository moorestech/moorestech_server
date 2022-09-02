﻿using System;
using System.Collections.Generic;
using Core.Item;
using Core.Item.Config;
using Game.PlayerInventory.Interface;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Server.Protocol.PacketResponse.Util.RecipePlace;
using Server.Util.MessagePack;

namespace Server.Protocol.PacketResponse
{
    public class SetRecipeCraftingInventoryProtocol: IPacketResponse
    {
        public const string Tag = "va:setRecipeCraftingInventory";
        private readonly IPlayerInventoryDataStore _playerInventoryDataStore;
        private readonly ItemStackFactory _itemStackFactory;
        private readonly IItemConfig _itemConfig;
        
        public SetRecipeCraftingInventoryProtocol(ServiceProvider serviceProvider)
        {
            _playerInventoryDataStore = serviceProvider.GetService<IPlayerInventoryDataStore>();
            _itemStackFactory = serviceProvider.GetService<ItemStackFactory>();
            _itemConfig = serviceProvider.GetService<IItemConfig>();
        }
        public List<List<byte>> GetResponse(List<byte> payload)
        {
            var data = MessagePackSerializer.Deserialize<SetRecipeCraftingInventoryProtocolMessagePack>(payload.ToArray());

            var mainInventory = _playerInventoryDataStore.GetInventoryData(data.PlayerId).MainOpenableInventory;
            var craftingInventory = _playerInventoryDataStore.GetInventoryData(data.PlayerId).CraftingOpenableInventory;
            var grabInventory = _playerInventoryDataStore.GetInventoryData(data.PlayerId).GrabInventory;
            
            
            //アイテムをすべてメインインベントリに移動
            MovingItemToMainInventory.Move(mainInventory,craftingInventory,grabInventory);
            
            //移動できるかチェック
            var (isReplaceable,mainInventoryRequiredItemCount) = CheckPlaceableRecipe.IsPlaceable(mainInventory,data.Recipe);
            if (!isReplaceable)
            {
                return new List<List<byte>>();
            }
            
            //実際に移動するアイテム数を計算
            var moveItem = CalcCraftInventoryPlaceItem.Calc(_itemStackFactory,_itemConfig,data.Recipe,mainInventoryRequiredItemCount);
            
            //実際に移動する
            MoveRecipeMainInventoryToCraftInventory.Move(_itemStackFactory, mainInventory, craftingInventory, moveItem);
            
            return new List<List<byte>>();
        }
    }
    
    [MessagePackObject(keyAsPropertyName :true)]
    public class SetRecipeCraftingInventoryProtocolMessagePack : ProtocolMessagePackBase
    {
        public SetRecipeCraftingInventoryProtocolMessagePack(int playerId,ItemMessagePack[] recipe)
        {
            Tag = SetRecipeCraftingInventoryProtocol.Tag;
            Recipe = recipe;
        }
        
        [Obsolete("デシリアライズ用のコンストラクタです。基本的に使用しないでください。")]
        public SetRecipeCraftingInventoryProtocolMessagePack() { }

        public ItemMessagePack[] Recipe { get; set; }
        public int PlayerId { get; set; }

    }
}