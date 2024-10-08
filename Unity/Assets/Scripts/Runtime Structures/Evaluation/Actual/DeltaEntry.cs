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
    public class DeltaEntry
    {
        public Combatant User;

        public ICombatantTarget Target;

        public int Intensity;

        public TokenEvaluatorBuilder MadeFromBuilder;
        public PlayerChoice ChoiceToMake => this?.MadeFromBuilder?.PlayerChoiceToMake;

        public TokenEvaluatorBuilder.IntensityKind IntensityKindType = TokenEvaluatorBuilder.IntensityKind.None;
        public TokenEvaluatorBuilder.NumberOfCardsRelation NumberOfCardsRelationType = TokenEvaluatorBuilder.NumberOfCardsRelation.None;
        public List<ElementResourceChange> ElementResourceChanges = new List<ElementResourceChange>();

        public StatusEffect StatusEffect;
        public List<Action<DeltaEntry>> ActionsToExecute = new List<Action<DeltaEntry>>();
        public IRealizedOperationScriptingToken RealizedOperationScriptingToken = null;

        public CardsEvaluatableValue RelevantCards = null;

        public readonly CampaignContext FromCampaign;

        /// <summary>
        /// An indicator of who the original target of the ability is.
        /// If an ability has a 'FoeTarget' as its original target, then it's a targetable card.
        /// If an ability has a 'FoeTarget' after something that isn't a FoeTarget, it becomes random.
        /// </summary>
        public ICombatantTarget OriginalTarget;

        public DeltaEntry(CampaignContext fromCampaign, Combatant user, ICombatantTarget target)
        {
            this.FromCampaign = fromCampaign;
            this.User = user;
            this.Target = target;

            this.MadeFromBuilder = new TokenEvaluatorBuilder(new ConceptualTokenEvaluatorBuilder(),
                fromCampaign,
                user,
                user,
                target);
        }

        public DeltaEntry(TokenEvaluatorBuilder builder)
        {
            this.FromCampaign = builder.Campaign;
            this.User = builder.User;
            this.Target = builder.Target;

            this.MadeFromBuilder = builder;
            this.Intensity = builder.Intensity;
            this.IntensityKindType = builder.IntensityKindType;
            this.ElementResourceChanges = builder.ElementResourceChanges;
            this.OriginalTarget = builder.OriginalTarget;
            this.StatusEffect = builder.StatusEffect;
            this.ActionsToExecute = builder.ActionsToExecute;
            this.RealizedOperationScriptingToken = builder.BasedOnConcept.RealizedOperationScriptingToken;
            this.RelevantCards = builder.RelevantCardsEvaluatable;
            this.NumberOfCardsRelationType = builder.NumberOfCardsRelationType;
        }

        public DeltaEntry(DeltaEntry spunFrom) : this(spunFrom.MadeFromBuilder)
        {
            this.User = spunFrom.User;
            this.Target = spunFrom.Target;
            this.RelevantCards = spunFrom.RelevantCards;
        }

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

        public int GetArgumentValue(string argument)
        {
            if (argument.ToLower() == "intensity")
            {
                return this.Intensity;
            }

            int evaluatedValue = 0;
            if (!(BaseScriptingToken.TryGetIntegerEvaluatableFromStrings(new List<string>() { argument }, out IEvaluatableValue<int> output, out _, allowNameMatch: true) && output.TryEvaluateValue(this.FromCampaign, this.MadeFromBuilder, out evaluatedValue)))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse argument {argument}.", GlobalUpdateUX.LogType.RuntimeError);
            }

            return evaluatedValue;
        }

        public DeltaEntry SetArgumentValue(string argument, int newValue)
        {
            if (argument.ToLower() == "intensity")
            {
                this.Intensity = newValue;
                return null;
            }

            if (ElementDatabase.TryGetElement(argument, out Element mappedElement))
            {
                DeltaEntry elementSet = new DeltaEntry(this);
                elementSet.ElementResourceChanges.Add(new ElementResourceChange(mappedElement, null, new ConstantEvaluatableValue<int>(newValue)));
                return elementSet;
            }
            else if (StatusEffectDatabase.TryGetStatusEffectById(argument, out StatusEffect mappedStatus))
            {
                DeltaEntry statusSet = new DeltaEntry(this);
                statusSet.StatusEffect = mappedStatus;
                statusSet.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.SetStatusEffect;
                statusSet.Intensity = newValue;
                return statusSet;
            }
            else
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to set argument value for {argument}.", GlobalUpdateUX.LogType.RuntimeError);
                return null;
            }
        }
    }
}