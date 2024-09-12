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
        public Dictionary<string, int> ElementRequirements = new Dictionary<string, int>();

        public StatusEffect StatusEffect;

        public GamestateDelta GetEffectiveDelta(CentralGameStateController gameStateController)
        {
            GamestateDelta delta = new GamestateDelta();

            if (!this.Intensity.TryEvaluateValue(gameStateController, this, out int evaluatedIntensity))
            {
                Debug.Log($"{nameof(TokenEvaluatorBuilder)} ({nameof(GetEffectiveDelta)}): Failed to evaluate {nameof(this.Intensity)}. Cannot have a delta.");
                return delta;
            }

            ICombatantTarget foundTarget = null;
            this.Target.TryEvaluateValue(gameStateController, this, out foundTarget);

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

        public bool MeetsElementRequirements(CombatContext combatContext)
        {
            foreach (string element in this.ElementRequirements.Keys)
            {
                if (!combatContext.MeetsElementRequirement(element, this.ElementRequirements[element]))
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

        public static TokenEvaluatorBuilder Continue(TokenEvaluatorBuilder previous)
        {
            TokenEvaluatorBuilder builder = new TokenEvaluatorBuilder();

            builder.User = previous.User;
            builder.Target = previous.Target;
            builder.OriginalTarget = previous.OriginalTarget;

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