namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class SetStatusEffectStacksScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> AmountOfStacks { get; private set; }
        public string StatusEffect { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "SETSTATUSEFFECTSTACKS";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.SetStatusEffect;
            tokenBuilder.Intensity = AmountOfStacks;
            tokenBuilder.StatusEffect = StatusEffectDatabase.GetModel(this.StatusEffect);
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> remainingArguments))
            {
                return false;
            }

            // There should be one argument remaining, the status effect
            if (remainingArguments.Count != 1)
            {
                return false;
            }

            string remainingArgument = remainingArguments[0];

            scriptingToken = new SetStatusEffectStacksScriptingToken()
            {
                AmountOfStacks = output,
                StatusEffect = remainingArgument
            };

            return true;
        }

        public override bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }

        public override bool RequiresTarget()
        {
            return true;
        }
    }
}