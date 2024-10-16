namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class DrainsElementScriptingToken : AliasScriptingToken
    {
        public override string ScriptingTokenIdentifier => "DRAINSELEMENT";

        protected override bool TryGetTokenWithArguments(List<string> arguments, IEffectOwner owner, out List<string> scriptingTokenText)
        {
            scriptingTokenText = null;

            if (!BaseScriptingToken.TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> remainingArguments))
            {
                return false;
            }

            if (remainingArguments.Count != 1)
            {
                return false;
            }

            scriptingTokenText = new List<string>()
            {
                $"[REQUIRESATLEASTELEMENT: {output.GetScriptingTokenText()} {remainingArguments[0]}]",
                $"[GAINELEMENT: -{output.GetScriptingTokenText()} {remainingArguments[0]}]"
            };
            return true;
        }
    }
}