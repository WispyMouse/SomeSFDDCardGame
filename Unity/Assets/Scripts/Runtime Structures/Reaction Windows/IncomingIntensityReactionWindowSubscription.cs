namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using static SFDDCards.Evaluation.Actual.TokenEvaluatorBuilder;

    public abstract class IncomingIntensityReactionWindowSubscription : ReactionWindowSubscription
    {
        public readonly Combatant Owner;
        public readonly IntensityKind IntensityKindValue = IntensityKind.None;

        protected IncomingIntensityReactionWindowSubscription(AppliedStatusEffect owningEffect, IntensityKind intensityKind)
        {
            this.Reactor = owningEffect;
            this.Owner = owningEffect.Owner;
            this.IntensityKindValue = intensityKind;
        }
    }
}