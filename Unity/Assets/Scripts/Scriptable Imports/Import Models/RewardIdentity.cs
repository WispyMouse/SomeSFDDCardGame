namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    [System.Serializable]
    public class RewardIdentity
    {
        public enum RewardIdentityKind
        {
            Card = 0,
            Artifact = 1,
            Currency = 2
        }

        public RewardIdentityKind IdentityKind = RewardIdentityKind.Card;
        public string RewardIdentifier = string.Empty;
        public string QuantityText = string.Empty;
    }
}