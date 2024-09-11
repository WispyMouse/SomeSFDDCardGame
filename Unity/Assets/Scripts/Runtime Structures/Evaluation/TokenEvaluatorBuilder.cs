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
            NumberOfCards = 3
        }

        public enum NumberOfCardsRelation
        {
            None = 0,
            Draw = 1
        }

        public List<IScriptingToken> AppliedTokens = new List<IScriptingToken>();

        public bool ShouldLaunch = false;

        public ICombatantTarget Target;
        public ICombatantTarget User;
        public ICombatantTarget TopOfEffectTarget;

        public IEvaluatableValue<int> Intensity;
        public IntensityKind IntensityKindType;
        public NumberOfCardsRelation NumberOfCardsRelationType = NumberOfCardsRelation.None;

        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();
        public Dictionary<string, int> ElementRequirements = new Dictionary<string, int>();

        public GamestateDelta GetEffectiveDelta(CentralGameStateController gameStateController)
        {
            GamestateDelta delta = new GamestateDelta();

            if (!this.Intensity.TryEvaluateValue(gameStateController, out int evaluatedIntensity))
            {
                Debug.Log($"{nameof(TokenEvaluatorBuilder)} ({nameof(GetEffectiveDelta)}): Failed to evaluate {nameof(this.Intensity)}. Cannot have a delta.");
                return delta;
            }

            delta.DeltaEntries.Add(new DeltaEntry()
            {
                User = this.User,
                Target = this.Target,
                Intensity = evaluatedIntensity,
                IntensityKindType = this.IntensityKindType,
                NumberOfCardsRelationType = this.NumberOfCardsRelationType,
                ElementResourceChanges = this.ElementResourceChanges,
                TopOfEffectTarget = this.TopOfEffectTarget
            }) ;

            return delta;
        }

        public GamestateDelta GetAbstractDelta()
        {
            GamestateDelta delta = new GamestateDelta();

            delta.DeltaEntries.Add(new DeltaEntry()
            {
                User = this.User,
                Target = this.Target,
                AbstractIntensity = this.Intensity,
                IntensityKindType = this.IntensityKindType,
                NumberOfCardsRelationType = this.NumberOfCardsRelationType,
                ElementResourceChanges = this.ElementResourceChanges,
                TopOfEffectTarget = this.TopOfEffectTarget
            });

            return delta;
        }

        public bool MeetsElementRequirements(CentralGameStateController gamestateController)
        {
            foreach (string element in this.ElementRequirements.Keys)
            {
                int requirementAmount = this.ElementRequirements[element];

                if (requirementAmount <= 0)
                {
                    continue;
                }

                if (gamestateController.ElementResourceCounts.TryGetValue(element, out int amountHeld))
                {
                    if (amountHeld < requirementAmount)
                    {
                        return false;
                    }
                }
                else
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

            foreach (string element in this.ElementRequirements.Keys)
            {
                if (this.ElementRequirements[element] <= 0)
                {
                    continue;
                }

                compositeRequirements.Append($"{startingComma}{this.ElementRequirements[element]} {element}");
                startingComma = ", ";
                nonzeroFound = true;
            }

            if (!nonzeroFound)
            {
                return string.Empty;
            }

            return compositeRequirements.ToString();
        }
    }
}