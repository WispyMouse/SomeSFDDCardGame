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
            ApplyStatusEffect = 4,
            RemoveStatusEffect = 5
        }

        public enum NumberOfCardsRelation
        {
            None = 0,
            Draw = 1
        }

        public CampaignContext Campaign;

        public List<IScriptingToken> AppliedTokens = new List<IScriptingToken>();

        public IEffectOwner Owner;
        public Combatant User;
        public ICombatantTarget OriginalTarget;
        public ICombatantTarget Target;

        public int Intensity;
        public IntensityKind IntensityKindType => this.BasedOnConcept.IntensityKindType;
        public NumberOfCardsRelation NumberOfCardsRelationType => this.BasedOnConcept.NumberOfCardsRelationType;

        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();
        public Dictionary<Element, int> ElementRequirements = new Dictionary<Element, int>();
        public List<IRequirement> Requirements => this.BasedOnConcept.Requirements;

        public StatusEffect StatusEffect => this.BasedOnConcept.StatusEffect;

        public List<Action<DeltaEntry>> ActionsToExecute => this.BasedOnConcept.ActionsToExecute;
        public TokenEvaluatorBuilder PreviousTokenBuilder = null;
        public ConceptualTokenEvaluatorBuilder BasedOnConcept = null;

        public TokenEvaluatorBuilder(ConceptualTokenEvaluatorBuilder concept, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget originalTarget, TokenEvaluatorBuilder previousBuilder = null)
        {
            this.Campaign = campaignContext;
            this.BasedOnConcept = concept;
            this.PreviousTokenBuilder = previousBuilder;
            this.Owner = owner;
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

        public bool MeetsRequirements(CombatContext combatContext)
        {
            foreach (IRequirement requirement in this.Requirements)
            {
                if (!requirement.MeetsRequirement(this, combatContext.FromCampaign))
                {
                    return false;
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

        public string GetIntensityDescriptionIfNotConstant()
        {
            if (this.BasedOnConcept == null)
            {
                return String.Empty;
            }

            if (this.BasedOnConcept.Intensity is ConstantEvaluatableValue<int>)
            {
                return String.Empty;
            }

            string descriptor = this.BasedOnConcept.Intensity.DescribeEvaluation();

            if (!string.IsNullOrEmpty(descriptor))
            {
                return $"({descriptor})";
            }

            return string.Empty;
        }
    }
}