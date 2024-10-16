namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;

    [Serializable]

    public class RewardImport : Importable
    {
        public List<PickRewardImport> PickRewards { get; set; } = new List<PickRewardImport>();
    }
}