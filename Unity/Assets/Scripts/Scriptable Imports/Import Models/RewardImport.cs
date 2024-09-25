namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;

    [Serializable]

    public class RewardImport
    {
        public string RewardId { get; set; }

        public List<PickRewardImport> PickRewards { get; set; } = new List<PickRewardImport>();
    }
}