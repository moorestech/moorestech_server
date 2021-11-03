﻿using System.Collections.Generic;
using Core.Block.Machine.util;
using Core.Util;
using Server.Util;
using World;

namespace Server.PacketHandle.PacketResponse
{
    public class PutBlockProtocol : IPacketResponse
    {
        private readonly WorldBlockDatastore _worldBlockDatastore;
        public PutBlockProtocol(WorldBlockDatastore worldBlockDatastore)
        {
            _worldBlockDatastore = worldBlockDatastore;
        }

        public List<byte[]> GetResponse(List<byte> payload)
        {
            //パケットのパース、接続元、接続先のインスタンス取得
            var payloadData = new ByteArrayEnumerator(payload);
            payloadData.MoveNextToGetShort();
            int blockId = payloadData.MoveNextToGetInt();
            payloadData.MoveNextToGetShort();
            int x = payloadData.MoveNextToGetInt();
            int y = payloadData.MoveNextToGetInt();
            
            var inputBlock = _worldBlockDatastore.GetBlockInventory(payloadData.MoveNextToGetInt());

            var block = NormalMachineFactory.Create(blockId, IntId.NewIntId(), _worldBlockDatastore.GetBlockInventory(payloadData.MoveNextToGetInt()));
            inputBlock.ChangeConnector(block);
            
            _worldBlockDatastore.AddBlock(block, x, y,block);
            //返すものはない
            return new List<byte[]>();
        }
    }
}