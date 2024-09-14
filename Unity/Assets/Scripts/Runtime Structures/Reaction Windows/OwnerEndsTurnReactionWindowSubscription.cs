namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class OwnerEndsTurnReactionWindowSubscription : ReactionWindowSubscription
    {
        public readonly Combatant Owner;
        public override string ReactionWindowId => KnownReactionWindows.OwnerEndsTurn;

        public OwnerEndsTurnReactionWindowSubscription(AppliedStatusEffect owningEffect)
        {
            this.Reactor = owningEffect;
            this.Owner = owningEffect.Owner;
        }

        public override bool ShouldApply(ReactionWindowContext context)
        {
            if (this.Owner == context.CombatantEffectOwner && this.Owner != null && this.Owner.CurrentHealth > 0)
            {
                return true;
            }

            return false;
        }
    }
}