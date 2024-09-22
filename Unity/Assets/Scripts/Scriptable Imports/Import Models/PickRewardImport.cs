namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    [System.Serializable]
    public class PickRewardImport
    {
        public enum PickRewardProtocol
        {
            ChooseX
        }

        public PickRewardProtocol Protocol = PickRewardProtocol.ChooseX;
        public int ProtocolArgument = 1;

        public List<RewardIdentity> RewardIdentities = new List<RewardIdentity>();
    }
}