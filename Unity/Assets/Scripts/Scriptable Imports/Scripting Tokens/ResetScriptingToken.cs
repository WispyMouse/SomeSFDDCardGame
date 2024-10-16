using SFDDCards.Evaluation.Actual;
using SFDDCards.Evaluation.Conceptual;
using SFDDCards.ScriptingTokens.EvaluatableValues;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class ResetScriptingToken : BaseScriptingToken
    {
        public override string ScriptingTokenIdentifier => "RESET";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.ElementRequirements.Clear();
            tokenBuilder.Target = tokenBuilder.OriginalTarget;
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.None;
            tokenBuilder.NumberOfCardsRelationType = TokenEvaluatorBuilder.NumberOfCardsRelation.None;
            tokenBuilder.Intensity = null;
            tokenBuilder.ElementResourceChanges.Clear();
            tokenBuilder.Requirements.Clear();
            tokenBuilder.RealizedOperationScriptingToken = null;
            tokenBuilder.RelevantCards = null;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = new ResetScriptingToken();
            return true;
        }
    }
}