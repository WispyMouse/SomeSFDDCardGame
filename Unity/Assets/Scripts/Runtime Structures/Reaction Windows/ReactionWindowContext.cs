namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using SFDDCards.Evaluation.Actual;

    public struct ReactionWindowContext
    {
        public string TimingWindowId;

        public Combatant CombatantEffectOwner;
        public ICombatantTarget CombatantTarget;

        public DeltaEntry ResultingDelta;
        public string PlayedFromZone;

        public ReactionWindowContext(string timingWindowId,
            DeltaEntry resultingDelta,
            string playedFromZone = null)
        {
            this.TimingWindowId = timingWindowId.ToLower();

            this.ResultingDelta = resultingDelta;
            this.CombatantEffectOwner = resultingDelta.User;
            this.CombatantTarget = resultingDelta.Target;

            this.PlayedFromZone = playedFromZone;
        }

        public ReactionWindowContext(string timingWindowId, 
            Combatant combatantEffectOwner,
            ICombatantTarget combatantTarget = null,
            string playedFromZone = null)
        {
            this.TimingWindowId = timingWindowId.ToLower();
            this.CombatantEffectOwner = combatantEffectOwner;
            this.CombatantTarget = combatantTarget;
            this.PlayedFromZone = playedFromZone;

            this.ResultingDelta = null;
        }
    }
}