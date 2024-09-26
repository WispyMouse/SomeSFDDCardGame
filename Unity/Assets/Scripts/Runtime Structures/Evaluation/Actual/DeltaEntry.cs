namespace SFDDCards.Evaluation.Actual
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
        public Combatant User;

        public ICombatantTarget Target;
        public CombatantTargetEvaluatableValue AbstractTarget;

        public int Intensity;
        public IEvaluatableValue<int> AbstractIntensity;

        public TokenEvaluatorBuilder MadeFromBuilder;

        public TokenEvaluatorBuilder.IntensityKind IntensityKindType = TokenEvaluatorBuilder.IntensityKind.None;
        public TokenEvaluatorBuilder.NumberOfCardsRelation NumberOfCardsRelationType = TokenEvaluatorBuilder.NumberOfCardsRelation.None;
        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();

        public StatusEffect StatusEffect;
        public List<Action<DeltaEntry>> ActionsToExecute = new List<Action<DeltaEntry>>();

        /// <summary>
        /// An indicator of who the original target of the ability is.
        /// If an ability has a 'FoeTarget' as its original target, then it's a targetable card.
        /// If an ability has a 'FoeTarget' after something that isn't a FoeTarget, it becomes random.
        /// </summary>
        public ICombatantTarget OriginalTarget;

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