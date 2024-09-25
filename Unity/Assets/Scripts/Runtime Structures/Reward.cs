namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class Reward
    {
        public string RewardId;

        public List<PickSomeReward> PickRewards = new List<PickSomeReward>();

        public Reward(RewardImport basedOn)
        {
            this.RewardId = basedOn.RewardId;
        }
    }
}