namespace SFDDCards.Evaluation.Actual
{
    using SFDDCards.Evaluation.Conceptual;
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

        public CampaignContext Campaign;

        public List<IScriptingToken> AppliedTokens = new List<IScriptingToken>();

        public ICombatantTarget User;
        public ICombatantTarget OriginalTarget;
        public ICombatantTarget Target;

        public int Intensity;
        public IntensityKind IntensityKindType => this.BasedOnConcept.IntensityKindType;
        public NumberOfCardsRelation NumberOfCardsRelationType => this.BasedOnConcept.NumberOfCardsRelationType;

        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();
        public Dictionary<Element, int> ElementRequirements = new Dictionary<Element, int>();
        public List<RequiresComparisonScriptingToken> RequiresComparisons => this.BasedOnConcept.RequiresComparisons;

        public StatusEffect StatusEffect => this.BasedOnConcept.StatusEffect;

        public List<Action<DeltaEntry>> ActionsToExecute => this.BasedOnConcept.ActionsToExecute;
        public TokenEvaluatorBuilder PreviousTokenBuilder = null;
        public ConceptualTokenEvaluatorBuilder BasedOnConcept = null;

        public TokenEvaluatorBuilder(ConceptualTokenEvaluatorBuilder concept, CampaignContext campaignContext, ICombatantTarget user, ICombatantTarget originalTarget, TokenEvaluatorBuilder previousBuilder = null)
        {
            this.Campaign = campaignContext;
            this.BasedOnConcept = concept;
            this.PreviousTokenBuilder = previousBuilder;

            this.User = user;
            this.OriginalTarget = originalTarget;

            if (concept.Target != null && !concept.Target.TryEvaluateValue(campaignContext, this, out this.Target))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Target cannot be evaluated, cannot resolve effect.", GlobalUpdateUX.LogType.RuntimeError);
            }

            if (concept.Intensity != null && !concept.Intensity.TryEvaluateValue(campaignContext, this, out this.Intensity))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Intensity cannot be evaluated, cannot resolve effect.", GlobalUpdateUX.LogType.RuntimeError);
            }
        }

        public GamestateDelta GetEffectiveDelta(CampaignContext campaignContext)
        {
            GamestateDelta delta = new GamestateDelta();

            delta.DeltaEntries.Add(new DeltaEntry()
            {
                MadeFromBuilder = this,
                User = this.User,
                Target = this.Target,
                Intensity = this.Intensity,
                IntensityKindType = this.IntensityKindType,
                NumberOfCardsRelationType = this.NumberOfCardsRelationType,
                ElementResourceChanges = this.ElementResourceChanges,
                OriginalTarget = this.OriginalTarget,
                StatusEffect = this.StatusEffect,
                ActionsToExecute = this.ActionsToExecute
            }) ;

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
                if (!this.ElementRequirements.TryGetValue(element, out int variable))
                {
                    GlobalUpdateUX.LogTextEvent?.Invoke($"Element requirements do not contain this element: {element}.", GlobalUpdateUX.LogType.RuntimeError);
                    return false;
                }

                if (!combatContext.MeetsElementRequirement(element, variable))
                {
                    return false;
                }
            }

            return true;
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