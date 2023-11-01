#if NET6_0
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Server.Protocol.PacketResponse;

namespace Test.CombinedTest.Server.PacketTest.Event
{
    public class EventTestUtil
    {
        public static List<byte> EventRequestData(int plyaerID)
        {
            return MessagePackSerializer.Serialize(new EventProtocolMessagePack(plyaerID)).ToList();
        }
    }
}
#endif