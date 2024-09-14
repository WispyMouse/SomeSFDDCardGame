using SFDDCards.ScriptingTokens.EvaluatableValues;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class ResetScriptingToken : BaseScriptingToken
    {
        public override string ScriptingTokenIdentifier => "RESET";

        public override void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.ElementRequirements.Clear();
            tokenBuilder.Target = new OriginalTargetEvaluatableValue();
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.None;
            tokenBuilder.NumberOfCardsRelationType = TokenEvaluatorBuilder.NumberOfCardsRelation.None;
            tokenBuilder.Intensity = null;
            tokenBuilder.ElementResourceChanges.Clear();
        }

        public override bool RequiresTarget()
        {
            return false;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = new ResetScriptingToken();
            return true;
        }
    }
}