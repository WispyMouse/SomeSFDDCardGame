namespace SFDDCards.Evaluation.Actual
{
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ImportModels;
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
            RemoveStatusEffect = 5,
            SetStatusEffect = 6,
            CurrencyMod = 7
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

        public IEvaluatableValue<int> Intensity;
        public IntensityKind IntensityKindType;
        public NumberOfCardsRelation NumberOfCardsRelationType => this.BasedOnConcept.NumberOfCardsRelationType;

        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();
        public Dictionary<Element, int> ElementRequirements = new Dictionary<Element, int>();
        public List<IRequirement> Requirements => this.BasedOnConcept.Requirements;

        public StatusEffect StatusEffect => this.BasedOnConcept.StatusEffect;
        public CurrencyImport Currency => this.BasedOnConcept.Currency;

        public List<Action<DeltaEntry>> ActionsToExecute => this.BasedOnConcept.ActionsToExecute;
        public TokenEvaluatorBuilder PreviousTokenBuilder = null;
        public ConceptualTokenEvaluatorBuilder BasedOnConcept = null;

        public PlayerChoice PlayerChoiceToMake => this.BasedOnConcept?.ChoiceToMake;
        public CardsEvaluatableValue RelevantCardsEvaluatable;

        public IRealizedOperationScriptingToken RealizedOperationScriptingToken = null;
        public string Destination = null;

        protected TokenEvaluatorBuilder(ConceptualTokenEvaluatorBuilder concept, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget originalTarget, TokenEvaluatorBuilder previousBuilder = null)
        {
            this.Campaign = campaignContext;
            this.BasedOnConcept = concept;
            this.PreviousTokenBuilder = previousBuilder;
            this.Owner = owner;
            this.User = user;
            this.OriginalTarget = originalTarget;
            this.Destination = concept.Destination;

            if (concept.Target != null)
            {
                if (!concept.Target.TryEvaluateValue(campaignContext, this, out this.Target))
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Target cannot be evaluated, cannot resolve effect.", GlobalUpdateUX.LogType.RuntimeError);
                }
            }
            else
            {
                this.Target = originalTarget;
            }

            if (this.BasedOnConcept?.RelevantCards != null)
            {
                this.RelevantCardsEvaluatable = this.BasedOnConcept?.RelevantCards;
            }
            else if (this.PreviousTokenBuilder != null && this.PreviousTokenBuilder.RelevantCardsEvaluatable != null)
            {
                this.RelevantCardsEvaluatable = this.PreviousTokenBuilder.RelevantCardsEvaluatable;
            }
        }

        public TokenEvaluatorBuilder(ConceptualTokenEvaluatorBuilder concept, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget originalTarget, List<ElementResourceChange> resourceChanges, TokenEvaluatorBuilder previousBuilder = null) : this(concept, campaignContext, owner, user, originalTarget, previousBuilder)
        {
            this.ElementResourceChanges = resourceChanges;
        }

        public TokenEvaluatorBuilder(ConceptualTokenEvaluatorBuilder concept, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget originalTarget, IEvaluatableValue<int> intensity, IntensityKind intensityKindType, TokenEvaluatorBuilder previousBuilder = null) : this(concept, campaignContext, owner, user, originalTarget, previousBuilder)
        {
            this.Intensity = intensity;
            this.IntensityKindType = intensityKindType;
        }

        public TokenEvaluatorBuilder(ConceptualTokenEvaluatorBuilder concept, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget originalTarget, List<ElementResourceChange> elementResourceChanges, IEvaluatableValue<int> intensity, IntensityKind intensityKindType, IRealizedOperationScriptingToken realizedOperationScriptingToken, TokenEvaluatorBuilder previousBuilder = null) : this(concept, campaignContext, owner, user, originalTarget, previousBuilder)
        {
            this.Intensity = intensity;
            this.IntensityKindType = intensityKindType;
            this.ElementResourceChanges = elementResourceChanges;
            this.RealizedOperationScriptingToken = realizedOperationScriptingToken;
        }

        public GamestateDelta GetEffectiveDelta(CampaignContext campaignContext)
        {
            GamestateDelta delta = new GamestateDelta();

            if (this.ElementResourceChanges != null && this.ElementResourceChanges.Count > 0)
            {
                delta.DeltaEntries.Add(new DeltaEntry(this)
                {
                    ElementResourceChanges = this.ElementResourceChanges
                });
            }

            if (this.Intensity != null)
            {
                delta.DeltaEntries.Add(new DeltaEntry(this)
                {
                    IntensityKindType = this.IntensityKindType,
                    ConceptualIntensity = this.Intensity,
                    StatusEffect = this.StatusEffect
                });
            }

            if (this.RealizedOperationScriptingToken != null)
            {
                delta.DeltaEntries.Add(new DeltaEntry(this)
                {
                    RealizedOperationScriptingToken = this.RealizedOperationScriptingToken
                });
            }

            if (this.ActionsToExecute != null && this.ActionsToExecute.Count > 0)
            {
                delta.DeltaEntries.Add(new DeltaEntry(this)
                {
                    ActionsToExecute = this.ActionsToExecute
                });
            }

            if (!string.IsNullOrEmpty(this.Destination))
            {
                delta.DeltaEntries.Add(new DeltaEntry(this)
                {
                    EncounterDialogueDestination = this.Destination
                });
            }

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
    }
}