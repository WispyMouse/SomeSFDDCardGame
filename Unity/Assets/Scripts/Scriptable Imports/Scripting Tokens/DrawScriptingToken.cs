namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class DrawScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> DrawAmount { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "DRAW";

        public override void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.NumberOfCards;
            tokenBuilder.NumberOfCardsRelationType = TokenEvaluatorBuilder.NumberOfCardsRelation.Draw;
            tokenBuilder.Intensity = DrawAmount;
            tokenBuilder.ShouldLaunch = true;
        }

        public override bool RequiresTarget()
        {
            return false;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // [DRAW: 5] or [DRAW: TARGET_HEALTH] or [DRAW: TARGET_HEALTH - 5] are all valid options
            // There should only be one resulting evaluated value out of this
            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> _))
            {
                return false;
            }

            scriptingToken = new DrawScriptingToken()
            {
                DrawAmount = output
            };

            return true;
        }
    }
}