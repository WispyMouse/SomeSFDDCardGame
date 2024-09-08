namespace SFDDCards
{
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
        public int Intensity;
        public TokenEvaluatorBuilder.IntensityKind IntensityKindType = TokenEvaluatorBuilder.IntensityKind.None;
        public TokenEvaluatorBuilder.NumberOfCardsRelation NumberOfCardsRelationType = TokenEvaluatorBuilder.NumberOfCardsRelation.None;
        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();

        /// <summary>
        /// An indicator of who the original target of the ability is.
        /// If an ability has a 'FoeTarget' as its original target, then it's a targetable card.
        /// If an ability has a 'FoeTarget' after something that isn't a FoeTarget, it becomes random.
        /// </summary>
        public ICombatantTarget TopOfEffectTarget;

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

            return null;
        }

        public string DescribeElementDelta()
        {
            StringBuilder elementDelta = new StringBuilder();

            foreach (ElementResourceChange change in this.ElementResourceChanges)
            {
                if (change.GainOrLoss > 0)
                {
                    elementDelta.AppendLine($"Gain {change.GainOrLoss} {change.Element}.");
                }
                else if (change.GainOrLoss < 0)
                {
                    elementDelta.AppendLine($"Lose {change.GainOrLoss} {change.Element}.");
                }
            }

            return elementDelta.ToString();
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
                compositeDelta.AppendLine(describeElementDelta);
            }

            string describeIntensityDelta = DescribeIntensityAsEffect();
            if (!string.IsNullOrEmpty(describeIntensityDelta))
            {
                compositeDelta.AppendLine(describeIntensityDelta);
            }

            return compositeDelta.ToString();
        }


        public string DescribeIntensityAsEffect()
        {
            if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
            {
                return $"Damages {DescribeTarget()} for {Intensity}";
            }
            else if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Heal)
            {
                return $"Heals {DescribeTarget()} for {Intensity}";
            }
            else if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.NumberOfCards)
            {
                if (NumberOfCardsRelationType == TokenEvaluatorBuilder.NumberOfCardsRelation.Draw)
                {
                    return $"Draw {Intensity} card(s)";
                }
            }

            return "I have no idea what this will do.";
        }

        public string DescribeTarget()
        {
            if (User == Target)
            {
                return "Self";
            }

            if (Target is FoeTarget)
            {
                if (User is Enemy)
                {
                    return "Player";
                }
                else if (Target == TopOfEffectTarget)
                {
                    return "Targeted Foe";
                }
                else
                {
                    if (User is AbstractPlayerUser)
                    {
                        return "Random Foe";
                    }
                    else if (User is Player)
                    {
                        return "Random Foe";
                    }
                    else
                    {
                        return "Player";
                    }
                }
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
    }
}