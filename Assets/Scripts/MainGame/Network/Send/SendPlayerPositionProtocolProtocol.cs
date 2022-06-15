using System.Collections.Generic;
using System.Linq;
using MainGame.Network.Settings;
using MainGame.Network.Util;
using MessagePack;
using Server.Protocol.PacketResponse;
using UnityEngine;

namespace MainGame.Network.Send
{
    public class SendPlayerPositionProtocolProtocol
    {
        private const short ProtocolId = 2;

        private readonly int _playerId;
        private readonly ISocketSender _socketSender;

        public SendPlayerPositionProtocolProtocol(ISocketSender socketSender,PlayerConnectionSetting playerConnectionSetting)
        {
            _socketSender = socketSender;
            _playerId = playerConnectionSetting.PlayerId;
        }
        public void Send(Vector2 pos)
        {
            _socketSender.Send(MessagePackSerializer.Serialize(new PlayerCoordinateSendProtocolMessagePack(
                _playerId,pos.x,pos.y)).ToList());
        }
    }
}