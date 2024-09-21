namespace SFDDCards.Evaluation.Conceptual
{
    using SFDDCards.Evaluation.Actual;
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
        
        public ConceptualDeltaEntry(ConceptualTokenEvaluatorBuilder builder, CombatantTargetEvaluatableValue originalConceptualTarget, CombatantTargetEvaluatableValue previousConceptualTarget)
        {
            this.MadeFromBuilder = builder;
            this.OriginalConceptualTarget = originalConceptualTarget;
            this.PreviousConceptualTarget = previousConceptualTarget;
            this.ConceptualTarget = previousConceptualTarget != null ? previousConceptualTarget : originalConceptualTarget;
        }

        /// <summary>
        /// Assuming that the delta has conceptual targets,
        /// describes the delta as though it were card text.
        /// </summary>
        /// <returns>A string description to log.</returns>
        public string DescribeAsEffect()
        {
            StringBuilder compositeDelta = new StringBuilder();

            string describeElementDelta = DescribeElementDelta();
            if (!string.IsNullOrEmpty(describeElementDelta))
            {
                compositeDelta.Append(describeElementDelta);
            }

            string describeIntensityDelta = DescribeEffect();
            if (!string.IsNullOrEmpty(describeIntensityDelta))
            {
                compositeDelta.Append(describeIntensityDelta);
            }

            return compositeDelta.ToString();
        }

        public string DescribeElementDelta()
        {
            StringBuilder elementDelta = new StringBuilder();

            foreach (ElementResourceChange change in this.ElementResourceChanges)
            {
                elementDelta.AppendLine($"Modify {change.Element.GetNameOrIcon()} by {change.GainOrLoss.DescribeEvaluation()}.");
            }

            return elementDelta.ToString();
        }

        public string DescribeEffect()
        {
            if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
            {
                return DescribeDamageDealtAsEffect();
            }
            else if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Heal)
            {
                return DescribeHealingAsEffect();
            }
            else if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.NumberOfCards)
            {
                if (NumberOfCardsRelationType == TokenEvaluatorBuilder.NumberOfCardsRelation.Draw)
                {
                    return $"Draw {DescribeIntensity()} card(s)";
                }
            }
            else if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.StatusEffect)
            {
                // TODO: This should know if it's gaining or losing stacks
                return $"Applies/Removes {DescribeIntensity()} stacks of {StatusEffect.Name}";
            }

            return String.Empty;
        }

        public string DescribeIntensity()
        {
            if (this.ConceptualIntensity == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"This ability lacks an intensity value.", GlobalUpdateUX.LogType.RuntimeError);
            }

            return this.ConceptualIntensity.DescribeEvaluation();
        }

        #region Describing Specific Effects
        public string DescribeDamageDealtAsEffect()
        {
            string intensity = this.DescribeIntensity();
            string targetAddOn = this.ConceptualTarget.DescribeEvaluation();
            bool includeTarget = true;

            if (!string.IsNullOrEmpty(targetAddOn))
            {
                targetAddOn = $" to {targetAddOn}";
            }

            // If the target is the same as the previous, then don't append the target
            if (this.PreviousConceptualTarget != null && this.PreviousConceptualTarget == this.ConceptualTarget)
            {
                includeTarget = false;
            }
            // If this the conceptual target is the same as the original target, and that target is one foe, omit target
            else if (this.PreviousConceptualTarget == null && this.ConceptualTarget == this.OriginalConceptualTarget && this.ConceptualTarget is FoeTargetEvaluatableValue)
            {
                includeTarget = false;
            }

            if (includeTarget)
            {
                return $"{intensity} damage{targetAddOn}.";
            }
            else
            {
                return $"{intensity} damage.";
            }
        }

        public string DescribeHealingAsEffect()
        {
            string intensity = this.DescribeIntensity();
            string targetAddOn = this.ConceptualTarget.DescribeEvaluation();
            bool includeTarget = true;

            if (!string.IsNullOrEmpty(targetAddOn))
            {
                targetAddOn = $" {targetAddOn} for ";
            }

            // If the target is the same as the previous, then don't append the target
            if (this.PreviousConceptualTarget != null && this.PreviousConceptualTarget == this.ConceptualTarget)
            {
                includeTarget = false;
            }
            // If this is the first entry (no previous entries) and the target is self, omit the target
            else if (this.PreviousConceptualTarget == null && this.ConceptualTarget == this.OriginalConceptualTarget && this.ConceptualTarget is SelfTargetEvaluatableValue)
            {
                includeTarget = false;
            }

            if (includeTarget)
            {
                return $"Heal{targetAddOn}{intensity}.";
            }
            else
            {
                return $"Heal {intensity}.";
            }
        }
        #endregion
    }
}