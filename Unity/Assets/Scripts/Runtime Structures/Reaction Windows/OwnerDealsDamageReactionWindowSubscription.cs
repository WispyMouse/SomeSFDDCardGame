namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class OwnerDealsDamageReactionWindowSubscription : ReactionWindowSubscription
    {
        public readonly Combatant Owner;
        public override string ReactionWindowId => KnownReactionWindows.DamageDealt;

        public OwnerDealsDamageReactionWindowSubscription(AppliedStatusEffect owningEffect)
        {
            this.Reactor = owningEffect;
            this.Owner = owningEffect.Owner;
        }

        public override bool ShouldApply(ReactionWindowContext context)
        {
            if (this.Owner == context.CombatantEffectOwner && this.Owner != null && this.Owner.CurrentHealth > 0
                && context.ResultingDelta != null && context.ResultingDelta.ConceptualIntensity.TryEvaluateValue(context.CampaignContext, context.ResultingDelta.MadeFromBuilder, out int value) && value > 0)
            {
                return true;
            }

            return false;
        }
    }
}