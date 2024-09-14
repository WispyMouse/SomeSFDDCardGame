namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class ChangeStatusEffectStacksScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> AmountOfStacks { get; private set; }
        public string StatusEffect { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "CHANGESTATUSEFFECTSTACKS";

        public override void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.StatusEffect;
            tokenBuilder.Intensity = AmountOfStacks;
            tokenBuilder.StatusEffect = StatusEffectDatabase.GetModel(this.StatusEffect);
            tokenBuilder.ShouldLaunch = true;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // [CHANGESTATUSEFFECTSTACKS: 5 POISON] or [CHANGESTATUSEFFECTSTACKS: TARGET_HEALTH POISON] or [CHANGESTATUSEFFECTSTACKS: TARGET_HEALTH + 1 POISON] are valid options
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

            scriptingToken = new ChangeStatusEffectStacksScriptingToken()
            {
                AmountOfStacks = output,
                StatusEffect = remainingArgument
            };

            return true;
        }

        public override bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            // Damage is always harmful!
            return true;
        }

        public override bool RequiresTarget()
        {
            return true;
        }
    }
}