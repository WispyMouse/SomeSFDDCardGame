namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CampaignContext
    {
        public readonly Deck CampaignDeck = new Deck();
        public CombatContext CurrentCombatContext { get; private set; } = null;

        public void AddCardToDeck(Card toAdd)
        {
            this.CampaignDeck.AddCardToDeck(toAdd);
        }

        public void LeaveCurrentCombat()
        {
            this.CurrentCombatContext = null;
        }

        public void StartCombat()
        {
            this.CurrentCombatContext = new CombatContext(this);
        }
    }
}