using System;
using Core.Master;
using Game.Context;
using Game.PlayerInventory.Interface;
using Mooresmaster.Model.ChallengesModule;
using UniRx;

namespace Game.Challenge.Task
{
    public class InInventoryItemChallengeTask : IChallengeTask
    {
        public ChallengeElement ChallengeElement { get; }
        public int PlayerId { get; }
        
        public IObservable<IChallengeTask> OnChallengeComplete => _onChallengeComplete;
        private readonly Subject<IChallengeTask> _onChallengeComplete = new();
        
        private bool _completed;
        
        private readonly InInventoryItemTaskParam _inInventoryItemTaskParam;
        private readonly PlayerInventoryData _playerInventory;
        
        public static IChallengeTask Create(int playerId, ChallengeElement challengeElement)
        {
            return new InInventoryItemChallengeTask(playerId, challengeElement);
        }
        public InInventoryItemChallengeTask(int playerId, ChallengeElement challengeElement)
        {
            ChallengeElement = challengeElement;
            PlayerId = playerId;
            
            _inInventoryItemTaskParam = (InInventoryItemTaskParam)challengeElement.TaskParam;
            _playerInventory = ServerContext.GetService<IPlayerInventoryDataStore>().GetInventoryData(playerId);
        }
        
        public void ManualUpdate()
        {
            if (_completed) return;
            
            var itemCount = 0;
            foreach (var item in _playerInventory.MainOpenableInventory.InventoryItems)
            {
                var taskItemId = MasterHolder.ItemMaster.GetItemId(_inInventoryItemTaskParam.ItemGuid);
                if (item.Id != taskItemId) continue;
                
                itemCount += item.Count;
                if (itemCount < _inInventoryItemTaskParam.ItemCount) continue;
                
                _onChallengeComplete.OnNext(this);
                _completed = true;
                break;
            }
        }
    }
}