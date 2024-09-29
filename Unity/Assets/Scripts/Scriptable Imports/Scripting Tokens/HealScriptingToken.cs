namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;

    public class HealScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> HealingAmount { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "HEAL";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.Heal;
            tokenBuilder.Intensity = HealingAmount;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // [HEAL: 5] or [HEAL: TARGET_HEALTH] or [HEAL: TARGET_HEALTH - 5] are all valid options
            // There should only be one resulting evaluated value out of this
            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> _))
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