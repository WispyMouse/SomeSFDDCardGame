namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class RemoveStatusEffectStacksScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> AmountOfStacks { get; private set; }
        public string StatusEffect { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "REMOVESTATUSEFFECTSTACKS";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            this.EnsureTarget(tokenBuilder);

            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.RemoveStatusEffect;
            tokenBuilder.Intensity = AmountOfStacks;
            tokenBuilder.StatusEffect = StatusEffectDatabase.GetModel(this.StatusEffect);
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // [APPLYSTATUSEFFECTSTACKS: 5 POISON] or [APPLYSTATUSEFFECTSTACKS: TARGET_HEALTH POISON] or [APPLYSTATUSEFFECTSTACKS: TARGET_HEALTH + 1 POISON] are valid options
            // We need to parse the strings for the stack amount and then the status effect
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

            scriptingToken = new RemoveStatusEffectStacksScriptingToken()
            {
                AmountOfStacks = output,
                StatusEffect = remainingArgument
            };

            return true;
        }
    }
}