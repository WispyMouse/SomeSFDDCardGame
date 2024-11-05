namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;

    public class GotoScriptingToken : BaseScriptingToken
    {
        public override string ScriptingTokenIdentifier { get; } = "GOTO";

        public string GotoIndex;

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Destination = GotoIndex;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            scriptingToken = new GotoScriptingToken()
            {
                GotoIndex = arguments[0]
            };

            return true;
        }
    }
}