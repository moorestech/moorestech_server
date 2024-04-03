using System.Collections.Generic;
using System.Linq;
using Game.Context;
using Game.Crafting.Interface;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Crafting.Config
{
    public class CraftConfigJsonLoad
    {
        public List<CraftingConfigData> Load(List<string> jsons)
        {
            var loadedData = jsons.SelectMany(JsonConvert.DeserializeObject<CraftConfigDataElement[]>).ToList();
            var itemStackFactory = ServerContext.ItemStackFactory;

            var result = new List<CraftingConfigData>();

            for (var i = 0; i < loadedData.Count; i++)
            {
                var config = loadedData[i];
                var items = new List<CraftingItemData>();
                foreach (var craftItem in config.Items)
                {
                    if (string.IsNullOrEmpty(craftItem.ItemName) || string.IsNullOrEmpty(craftItem.ModId))
                    {
                        items.Add(new CraftingItemData(itemStackFactory.CreatEmpty(), false));
                        continue;
                    }

                    items.Add(new CraftingItemData(
                        itemStackFactory.Create(craftItem.ModId, craftItem.ItemName, craftItem.Count),
                        craftItem.IsRemain));
                }

                //TODO ロードした時にあるべきものがなくnullだったらエラーを出す
                if (config.Result.ModId == null) Debug.Log(i + " : Result item is null");

                var resultItem = itemStackFactory.Create(config.Result.ModId, config.Result.ItemName, config.Result.Count);

                result.Add(new CraftingConfigData(items, resultItem, i));
            }

            return result;
        }
    }
}