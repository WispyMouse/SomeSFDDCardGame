namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using static SFDDCards.Evaluation.Actual.TokenEvaluatorBuilder;

    public class IncomingDamageReactionWindowSubscription : IncomingIntensityReactionWindowSubscription
    {
        public override string ReactionWindowId => KnownReactionWindows.DamageIncoming;

        public IncomingDamageReactionWindowSubscription(AppliedStatusEffect owningEffect) : base(owningEffect, IntensityKind.Damage)
        {
        }
    }
}