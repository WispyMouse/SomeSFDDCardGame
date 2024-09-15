namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public class TokenEvaluatorBuilder
    {
        public enum IntensityKind
        {
            None = 0,
            Damage = 1,
            Heal = 2,
            NumberOfCards = 3,
            StatusEffect = 4
        }

        public enum NumberOfCardsRelation
        {
            None = 0,
            Draw = 1
        }

        public List<IScriptingToken> AppliedTokens = new List<IScriptingToken>();

        public bool ShouldLaunch = false;

        public CombatantTargetEvaluatableValue Target;
        public ICombatantTarget User;
        public ICombatantTarget OriginalTarget;

        public IEvaluatableValue<int> Intensity;
        public IntensityKind IntensityKindType;
        public NumberOfCardsRelation NumberOfCardsRelationType = NumberOfCardsRelation.None;

        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();
        public Dictionary<Element, IEvaluatableValue<int>> ElementRequirements = new Dictionary<Element, IEvaluatableValue<int>>();
        public List<RequiresComparisonScriptingToken> RequiresComparisons = new List<RequiresComparisonScriptingToken>();

        public StatusEffect StatusEffect;
        public string LogText;

        public List<Action<TokenEvaluatorBuilder>> ActionsToExecute = new List<Action<TokenEvaluatorBuilder>>();

        public TokenEvaluatorBuilder(ICombatantTarget inUser, ICombatantTarget inOriginalTarget)
        {
            this.User = inUser;
            this.OriginalTarget = inOriginalTarget;
        }

        public GamestateDelta GetEffectiveDelta(CampaignContext campaignContext)
        {
            GamestateDelta delta = new GamestateDelta();
            int evaluatedIntensity = 0;

            if (this.Intensity != null)
            {
                if (!this.Intensity.TryEvaluateValue(campaignContext, this, out evaluatedIntensity))
                {
                    GlobalUpdateUX.LogTextEvent?.Invoke($"{nameof(TokenEvaluatorBuilder)} ({nameof(GetEffectiveDelta)}): Failed to evaluate {nameof(this.Intensity)}. Cannot have a delta.", GlobalUpdateUX.LogType.RuntimeError);
                    return delta;
                }
            }

            ICombatantTarget foundTarget = null;

            if (!(this.Target != null && this.Target.TryEvaluateValue(campaignContext, this, out foundTarget)))
            {
                GlobalUpdateUX.LogTextEvent?.Invoke($"Failed to evaluate a target for this ability", GlobalUpdateUX.LogType.RuntimeError);
            }

            delta.DeltaEntries.Add(new DeltaEntry()
            {
                MadeFromBuilder = this,
                User = this.User,
                Target = foundTarget,
                Intensity = evaluatedIntensity,
                IntensityKindType = this.IntensityKindType,
                NumberOfCardsRelationType = this.NumberOfCardsRelationType,
                ElementResourceChanges = this.ElementResourceChanges,
                OriginalTarget = this.OriginalTarget,
                StatusEffect = this.StatusEffect
            }) ;

            return delta;
        }

        public GamestateDelta GetAbstractDelta()
        {
            GamestateDelta delta = new GamestateDelta();

            delta.DeltaEntries.Add(new DeltaEntry()
            {
                MadeFromBuilder = this,
                User = this.User,
                AbstractTarget = this.Target,
                AbstractIntensity = this.Intensity,
                IntensityKindType = this.IntensityKindType,
                NumberOfCardsRelationType = this.NumberOfCardsRelationType,
                ElementResourceChanges = this.ElementResourceChanges,
                OriginalTarget = this.OriginalTarget,
                StatusEffect = this.StatusEffect
            });

            return delta;
        }

        public bool MeetsComparisonRequirements(CombatContext combatContext)
        {
            foreach (RequiresComparisonScriptingToken comparison in this.RequiresComparisons)
            {
                if (!comparison.Left.TryEvaluateValue(combatContext.FromCampaign, this, out int leftValue))
                {
                    return false;
                }

                if (!comparison.Right.TryEvaluateValue(combatContext.FromCampaign, this, out int rightValue))
                {
                    return false;
                }

                bool evaluationResult = false;

                switch (comparison.ComparisonType)
                {
                    case RequiresComparisonScriptingToken.Comparison.LessThan:
                        evaluationResult = leftValue < rightValue;
                        break;
                    case RequiresComparisonScriptingToken.Comparison.LessThanOrEqual:
                        evaluationResult = leftValue <= rightValue;
                        break;
                    case RequiresComparisonScriptingToken.Comparison.EqualTo:
                        evaluationResult = leftValue == rightValue;
                        break;
                    case RequiresComparisonScriptingToken.Comparison.GreaterThan:
                        evaluationResult = leftValue > rightValue;
                        break;
                    case RequiresComparisonScriptingToken.Comparison.GreaterThanOrEqual:
                        evaluationResult = leftValue >= rightValue;
                        break;
                }
            }

            return true;
        }

        public bool MeetsElementRequirements(CombatContext combatContext)
        {
            foreach (Element element in this.ElementRequirements.Keys)
            {
                if (!this.ElementRequirements.TryGetValue(element, out IEvaluatableValue<int> variable))
                {
                    GlobalUpdateUX.LogTextEvent?.Invoke($"Could not parse a variable for meeting requirements.", GlobalUpdateUX.LogType.RuntimeError);
                    return false;
                }

                if (!variable.TryEvaluateValue(combatContext.FromCampaign, this, out int evaluatedValue))
                {
                    GlobalUpdateUX.LogTextEvent?.Invoke($"Failed to evaluate value while trying to meet requirements.", GlobalUpdateUX.LogType.RuntimeError);
                    return false;
                }

                if (!combatContext.MeetsElementRequirement(element, evaluatedValue))
                {
                    return false;
                }
            }

            return true;
        }

        public string DescribeElementRequirements()
        {
            if (this.ElementRequirements.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder compositeRequirements = new StringBuilder();
            compositeRequirements.Append("Requires: ");
            string startingComma = "";
            bool nonzeroFound = false;

            foreach (Element element in this.ElementRequirements.Keys)
            {
                compositeRequirements.Append($"{startingComma}{this.ElementRequirements[element].DescribeEvaluation()} {element.GetNameOrIcon()}");
                startingComma = ", ";
                nonzeroFound = true;
            }

            if (!nonzeroFound)
            {
                return string.Empty;
            }

            return compositeRequirements.ToString();
        }

        public static TokenEvaluatorBuilder Continue(TokenEvaluatorBuilder previous)
        {
            TokenEvaluatorBuilder builder = new TokenEvaluatorBuilder(previous.User, previous.OriginalTarget);

            builder.Target = previous.Target;

            return builder;
        }

        public bool AnyEffectRequiresTarget()
        {
            for (int ii = 0; ii < this.AppliedTokens.Count; ii++)
            {
                if (this.AppliedTokens[ii].RequiresTarget())
                {
                    return true;
                }
            }

            return false;
        }
    }
}