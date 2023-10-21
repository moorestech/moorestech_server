using System;
using System.Linq;
using Game.Quest.Interface.Event;
using MessagePack;

namespace Server.Event.EventReceive
{
    public class QuestCompletedToSendEventPacket
    {
        public const string EventTag = "va:event:questCompleted";

        private readonly EventProtocolProvider _eventProtocolProvider;

        public QuestCompletedToSendEventPacket(IQuestCompletedEvent questCompletedEvent, EventProtocolProvider eventProtocolProvider)
        {
            _eventProtocolProvider = eventProtocolProvider;
            questCompletedEvent.SubscribeCompletedId(OnQuestCompleted);
        }

        private void OnQuestCompleted((int playerId, string questId) args)
        {
            var packet = MessagePackSerializer.Serialize(new QuestCompletedEventMessagePack(args.questId));
            _eventProtocolProvider.AddEvent(args.playerId, packet.ToList());
        }
    }

    [MessagePackObject(true)]
    public class QuestCompletedEventMessagePack : EventProtocolMessagePackBase
    {
        [Obsolete("。。")]
        public QuestCompletedEventMessagePack()
        {
        }

        public QuestCompletedEventMessagePack(string questId)
        {
            EventTag = PlaceBlockToSetEventPacket.EventTag;
            QuestId = questId;
        }

        public string QuestId { get; set; }
    }
}