namespace SFDDCards.Evaluation.Conceptual
{
    using SFDDCards.Evaluation.Actual;
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

        public IEvaluatableValue<int> Intensity;
        public IntensityKind IntensityKindType;
        public NumberOfCardsRelation NumberOfCardsRelationType = NumberOfCardsRelation.None;

        public StatusEffect StatusEffect;

        public ConceptualTokenEvaluatorBuilder PreviousBuilder;
        public List<Action<DeltaEntry>> ActionsToExecute = new List<Action<DeltaEntry>>();
        public IRealizedOperationScriptingToken RealizedOperationScriptingToken = null;
        public ReactionWindowContext? CreatedFromContext;

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
            }
        }

        public ConceptualTokenEvaluatorBuilder(ReactionWindowContext? context, ConceptualTokenEvaluatorBuilder previousBuilder = null) : this(previousBuilder)
        {
            this.CreatedFromContext = context;
        }

        public bool HasSameElementRequirement(ConceptualTokenEvaluatorBuilder previous)
        {
            if (previous == null)
            {
                return false;
            }

            if (this.ElementRequirements.Count != previous.ElementRequirements.Count)
            {
                return false;
            }

            foreach (Element elementKey in this.ElementRequirements.Keys)
            {
                if (!previous.ElementRequirements.TryGetValue(elementKey, out IEvaluatableValue<int> value))
                {
                    return false;
                }

                if (value != this.ElementRequirements[elementKey])
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
            string startingComma = "";
            bool nonzeroFound = false;

            foreach (Element element in this.ElementRequirements.Keys)
            {
                compositeRequirements.Append($"{startingComma}{this.ElementRequirements[element].DescribeEvaluation()} {element.GetNameAndMaybeIcon()}");
                startingComma = ", ";
                nonzeroFound = true;
            }

            if (!nonzeroFound)
            {
                return string.Empty;
            }

            compositeRequirements.Append(":");

            return compositeRequirements.ToString().Trim();
        }

        public ConceptualDelta GetConceptualDelta()
        {
            ConceptualDelta delta = new ConceptualDelta();

            delta.DeltaEntries.Add(new ConceptualDeltaEntry(this, this.OriginalTarget, this.PreviousBuilder?.Target)
            {
                MadeFromBuilder = this,
                ConceptualTarget = this.Target,
                ConceptualIntensity = this.Intensity,
                IntensityKindType = this.IntensityKindType,
                NumberOfCardsRelationType = this.NumberOfCardsRelationType,
                ElementResourceChanges = this.ElementResourceChanges,
                StatusEffect = this.StatusEffect
            });

            return delta;
        }
    }
}