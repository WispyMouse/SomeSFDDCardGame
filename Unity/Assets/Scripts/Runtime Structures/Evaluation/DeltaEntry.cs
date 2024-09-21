namespace SFDDCards
{
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    public class DeltaEntry
    {
        public ICombatantTarget User;

        public ICombatantTarget Target;
        public CombatantTargetEvaluatableValue AbstractTarget;

        public int Intensity;
        public IEvaluatableValue<int> AbstractIntensity;

        public TokenEvaluatorBuilder MadeFromBuilder;

        public TokenEvaluatorBuilder.IntensityKind IntensityKindType = TokenEvaluatorBuilder.IntensityKind.None;
        public TokenEvaluatorBuilder.NumberOfCardsRelation NumberOfCardsRelationType = TokenEvaluatorBuilder.NumberOfCardsRelation.None;
        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();

        public StatusEffect StatusEffect;

        /// <summary>
        /// An indicator of who the original target of the ability is.
        /// If an ability has a 'FoeTarget' as its original target, then it's a targetable card.
        /// If an ability has a 'FoeTarget' after something that isn't a FoeTarget, it becomes random.
        /// </summary>
        public ICombatantTarget OriginalTarget;

        /// <summary>
        /// Assuming that the delta is being applied, this describes
        /// the actual effect on the targets of the ability.
        /// </summary>
        /// <returns>A string description to log.</returns>
        public string DescribeDelta()
        {
            StringBuilder compositeDelta = new StringBuilder();

            string describeElementDelta = DescribeElementDelta();
            if (!string.IsNullOrEmpty(describeElementDelta))
            {
                compositeDelta.AppendLine(describeElementDelta);
            }

            string describeIntensityDelta = DescribeIntensityDelta();
            if (!string.IsNullOrEmpty(describeIntensityDelta))
            {
                compositeDelta.AppendLine(describeIntensityDelta);
            }
            
            return compositeDelta.ToString();
        }

        public string DescribeIntensityDelta()
        {
            if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
            {
                return $"{User.Name} damages {DescribeTarget()} for {Intensity}";
            }
            else if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Heal)
            {
                return $"{User.Name} heals {DescribeTarget()} for {Intensity}";
            }
            else if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.NumberOfCards)
            {
                if (NumberOfCardsRelationType == TokenEvaluatorBuilder.NumberOfCardsRelation.Draw)
                {
                    return $"Player draws {Intensity} card(s)";
                }
            }
            else if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.StatusEffect)
            {
                if (Intensity > 0)
                {
                    return $"{DescribeTarget()} has {Intensity} stacks of {StatusEffect} applied";
                }
                else if (Intensity < 0)
                {
                    return $"{DescribeTarget()} has {-Intensity} stacks of {StatusEffect} removed";
                }
                else
                {
                    return null;
                }
            }

            return null;
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

        public string DescribeTarget()
        {
            if (AbstractTarget != null)
            {
                return AbstractTarget.DescribeEvaluation();
            }

            if (Target == null)
            {
                return "No Target";
            }

            if (User == null)
            {
                Debug.LogError($"User is null! This is unusual.");
                return String.Empty;
            }

            if (User == Target)
            {
                return "Self";
            }

            if (User == null && Target == null)
            {
                return "Any Target";
            }

            if (Target == null)
            {
                return "?";
            }

            return Target.Name;
        }

        public string DescribeIntensity()
        {
            if (this.AbstractIntensity == null)
            {
                return this.Intensity.ToString();
            }

            return this.AbstractIntensity.DescribeEvaluation();
        }

        public bool IsTargetingOriginalTarget
        {
            get
            {
                if (this.OriginalTarget == null || this.Target == null)
                {
                    return false;
                }

                return this.OriginalTarget == this.Target;
            }
        }
    }
}