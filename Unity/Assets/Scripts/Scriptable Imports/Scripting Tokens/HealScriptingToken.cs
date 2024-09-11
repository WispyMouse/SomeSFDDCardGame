namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class HealScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> HealingAmount { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "HEAL";

        public override void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.Heal;
            tokenBuilder.Intensity = HealingAmount;
            tokenBuilder.ShouldLaunch = true;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // [HEAL: 5] or [HEAL: TARGET_HEALTH] or [HEAL: TARGET_HEALTH - 5] are all valid options
            // There should only be one resulting evaluated value out of this
            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output))
            {
                return false;
            }

            scriptingToken = new HealScriptingToken()
            {
                HealingAmount = output
            };

            return true;
        }
    }
}