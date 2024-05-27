using System;
using System.Collections.Generic;
using Game.Block.Interface;
using Game.Block.Interface.Component;
using Game.Block.Interface.State;
using UnityEngine;

namespace Game.World.Interface.DataStore
{
    public interface IWorldBlockDatastore
    {
        public IReadOnlyDictionary<int, WorldBlockData> BlockMasterDictionary { get; }

        public IObservable<(ChangedBlockState state, WorldBlockData blockData)> OnBlockStateChange { get; }

        public bool AddBlock(IBlock block);
        public bool RemoveBlock(Vector3Int pos);
        public IBlock GetBlock(Vector3Int pos);
        public WorldBlockData GetOriginPosBlock(Vector3Int pos);
        public Vector3Int GetBlockPosition(int entityId);
        public BlockDirection GetBlockDirection(Vector3Int pos);
        public List<BlockJsonObject> GetSaveJsonObject();
        public void LoadBlockDataList(List<BlockJsonObject> saveBlockDataList);
    }

    public static class WorldBlockDatastoreExtension
    {
        public static bool Exists(this IWorldBlockDatastore datastore, Vector3Int pos)
        {
            var block = datastore.GetBlock(pos);
            return block != null;
        }

        public static bool TryGetBlock(this IWorldBlockDatastore datastore, Vector3Int pos, out IBlock block)
        {
            block = datastore.GetBlock(pos);
            return block != null;
        }

        public static bool ExistsComponent<TComponent>(this IWorldBlockDatastore datastore, Vector3Int pos) where TComponent : IBlockComponent
        {
            var block = datastore.GetBlock(pos);
            if (block == null)
            {
                return false;
            }
            return block.ComponentManager.ExistsComponent<TComponent>();
        }

        public static TComponent GetBlock<TComponent>(this IWorldBlockDatastore datastore, Vector3Int pos) where TComponent : IBlockComponent
        {
            var block = datastore.GetBlock(pos);

            if (block.ComponentManager.TryGetComponent(out TComponent component2)) return component2;

            return default;
        }

        public static bool TryGetBlock<TComponent>(this IWorldBlockDatastore datastore, Vector3Int pos, out TComponent component) where TComponent : IBlockComponent
        {
            if (datastore.ExistsComponent<TComponent>(pos))
            {
                component = datastore.GetBlock<TComponent>(pos);
                return true;
            }

            component = default;
            return false;
        }
    }
}