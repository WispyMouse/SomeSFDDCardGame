namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;


    public struct ReactionWindowContext
    {
        public string TimingWindowId;

        public Combatant CombatantEffectOwner;

        public ReactionWindowContext(string timingWindowId, Combatant combatantEffectOwner)
        {
            this.TimingWindowId = timingWindowId.ToLower();
            this.CombatantEffectOwner = combatantEffectOwner;
        }
    }
}