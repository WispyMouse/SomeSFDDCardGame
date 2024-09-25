namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using static SFDDCards.PickRewardImport;

    public class PickSomeReward
    {
        public readonly PickRewardImport BasedOn;

        public PickRewardProtocol Protocol => this.BasedOn.Protocol;
        public List<PickSomeRewardSlot> PickRewardSlots = new List<PickSomeRewardSlot>();

        public PickSomeReward(PickRewardImport basedOn)
        {
            this.BasedOn = basedOn;
        }
    }
}