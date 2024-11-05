namespace SFDDCards.Evaluation.Conceptual
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Describes a one gamestate change that would result from an effect being used, conceptually.
    /// This is meant to describe an effect in concept, but not with actual targets.
    /// This can be used to describe individual effects of cards and status effects, while they aren't being played.
    /// </summary>
    public class ConceptualDeltaEntry
    {
        public CombatantTargetEvaluatableValue ConceptualTarget;
        public CombatantTargetEvaluatableValue OriginalConceptualTarget;
        public CombatantTargetEvaluatableValue PreviousConceptualTarget;

        public IEvaluatableValue<int> ConceptualIntensity;

        public ConceptualTokenEvaluatorBuilder MadeFromBuilder;

        public TokenEvaluatorBuilder.IntensityKind IntensityKindType = TokenEvaluatorBuilder.IntensityKind.None;
        public TokenEvaluatorBuilder.NumberOfCardsRelation NumberOfCardsRelationType = TokenEvaluatorBuilder.NumberOfCardsRelation.None;
        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();

        public StatusEffect StatusEffect;
        public CurrencyImport Currency;

        public string Destination;

        public ConceptualDeltaEntry(ConceptualTokenEvaluatorBuilder builder, CombatantTargetEvaluatableValue originalConceptualTarget, CombatantTargetEvaluatableValue previousConceptualTarget)
        {
            this.MadeFromBuilder = builder;
            this.OriginalConceptualTarget = originalConceptualTarget;
            this.PreviousConceptualTarget = previousConceptualTarget;
            this.ConceptualTarget = previousConceptualTarget != null ? previousConceptualTarget : originalConceptualTarget;
        }
    }
}