namespace SFDDCards.Evaluation.Conceptual
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using static SFDDCards.Evaluation.Actual.TokenEvaluatorBuilder;

    public class ConceptualTokenEvaluatorBuilder
    {
        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();
        public List<IScriptingToken> AppliedTokens = new List<IScriptingToken>();

        public List<IRequirement> Requirements = new List<IRequirement>();
        public Dictionary<Element, IEvaluatableValue<int>> ElementRequirements = new Dictionary<Element, IEvaluatableValue<int>>();

        public CombatantTargetEvaluatableValue Target;
        public CombatantTargetEvaluatableValue OriginalTarget;
        public IEffectOwner Owner;
        public string PlayedFromZone => this?.CreatedFromContext?.PlayedFromZone;

        public IEvaluatableValue<int> Intensity;
        public IntensityKind IntensityKindType;
        public NumberOfCardsRelation NumberOfCardsRelationType = NumberOfCardsRelation.None;

        public StatusEffect StatusEffect;
        public CurrencyImport Currency;

        public ConceptualTokenEvaluatorBuilder PreviousBuilder;
        public List<Action<DeltaEntry>> ActionsToExecute = new List<Action<DeltaEntry>>();
        public IRealizedOperationScriptingToken RealizedOperationScriptingToken;
        public ReactionWindowContext? CreatedFromContext;

        public CardsEvaluatableValue RelevantCards = null;
        public PlayerChoice ChoiceToMake = null;

        public string Destination;

        public ConceptualTokenEvaluatorBuilder(ConceptualTokenEvaluatorBuilder previousBuilder = null)
        {
            this.PreviousBuilder = previousBuilder;

            if (PreviousBuilder != null)
            {
                this.ElementRequirements = new Dictionary<Element, IEvaluatableValue<int>>(previousBuilder.ElementRequirements);
                this.OriginalTarget = previousBuilder.OriginalTarget;
                this.Target = previousBuilder.Target;
                this.Owner = previousBuilder.Owner;
                this.CreatedFromContext = previousBuilder.CreatedFromContext;
                this.RelevantCards = previousBuilder.RelevantCards;
                this.Requirements = new List<IRequirement>(previousBuilder.Requirements);
            }

            this.CreatedFromContext = previousBuilder?.CreatedFromContext;
        }

        public ConceptualTokenEvaluatorBuilder(ReactionWindowContext? context, ConceptualTokenEvaluatorBuilder previousBuilder = null) : this(previousBuilder)
        {
            this.CreatedFromContext = context;
        }

        public bool HasSameRequirements(ConceptualTokenEvaluatorBuilder previous)
        {
            if (previous == null)
            {
                if (this.ElementRequirements.Count == 0 && this.Requirements.Count == 0)
                {
                    return true;
                }

                return false;
            }

            if (this.ElementRequirements.Count != this.PreviousBuilder.ElementRequirements.Count)
            {
                return false;
            }

            if (this.Requirements.Count != this.PreviousBuilder.Requirements.Count)
            {
                return false;
            }

            foreach (Element elementKey in this.ElementRequirements.Keys)
            {
                if (!this.PreviousBuilder.ElementRequirements.TryGetValue(elementKey, out IEvaluatableValue<int> value))
                {
                    return false;
                }

                if (value != this.ElementRequirements[elementKey])
                {
                    return false;
                }
            }

            for (int ii = 0; ii < this.Requirements.Count; ii++)
            {
                if (this.Requirements[ii] != this.PreviousBuilder.Requirements[ii])
                {
                    return false;
                }
            }

            return true;
        }

        public ConceptualDelta GetConceptualDelta()
        {
            ConceptualDelta delta = new ConceptualDelta()
            {
                Owner = this.Owner
            };

            if (this.ElementResourceChanges != null && this.ElementResourceChanges.Count > 0)
            {
                delta.DeltaEntries.Add(new ConceptualDeltaEntry(this, this.OriginalTarget, this.PreviousBuilder?.Target)
                {
                    MadeFromBuilder = this,
                    ConceptualTarget = this.Target,
                    IntensityKindType = IntensityKind.None,
                    NumberOfCardsRelationType = NumberOfCardsRelation.None,
                    ElementResourceChanges = this.ElementResourceChanges
                });
            }

            if (this.Intensity != null)
            {
                delta.DeltaEntries.Add(new ConceptualDeltaEntry(this, this.OriginalTarget, this.PreviousBuilder?.Target)
                {
                    MadeFromBuilder = this,
                    ConceptualTarget = this.Target,
                    ConceptualIntensity = this.Intensity,
                    IntensityKindType = this.IntensityKindType,
                    NumberOfCardsRelationType = this.NumberOfCardsRelationType,
                    StatusEffect = this.StatusEffect,
                    Currency = this.Currency
                });
            }

            if (this.RealizedOperationScriptingToken != null)
            {
                delta.DeltaEntries.Add(new ConceptualDeltaEntry(this, this.OriginalTarget, this.PreviousBuilder?.Target)
                {
                    MadeFromBuilder = this,
                    IntensityKindType = IntensityKind.None,
                    NumberOfCardsRelationType = NumberOfCardsRelation.None
                });
            }

            if (!string.IsNullOrEmpty(this.Destination))
            {
                delta.DeltaEntries.Add(new ConceptualDeltaEntry(this, this.OriginalTarget, this.PreviousBuilder?.Target)
                {
                    MadeFromBuilder = this,
                    IntensityKindType = IntensityKind.None,
                    NumberOfCardsRelationType = NumberOfCardsRelation.None,
                    Destination = this.Destination
                });
            }

            return delta;
        }

        public bool ShouldLaunch
        {
            get
            {
                return
                    (this.Intensity != null && this.IntensityKindType != IntensityKind.None)
                    || (this.RealizedOperationScriptingToken != null)
                    || (this.ChoiceToMake != null)
                    || (this.ActionsToExecute.Count > 0)
                    || (this.ElementResourceChanges.Count > 0)
                    || (!string.IsNullOrEmpty(this.Destination));
            }
        }
    }
}