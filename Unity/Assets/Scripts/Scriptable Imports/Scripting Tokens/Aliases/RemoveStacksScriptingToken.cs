namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class RemoveStacksScriptingToken : AliasScriptingToken
    {
        public override string ScriptingTokenIdentifier => "REMOVESTACKS";

        protected override bool TryGetTokenWithArguments(List<string> arguments, IEffectOwner owner, out List<string> scriptingTokenText)
        {
            scriptingTokenText = null;

            if (owner == null || !(owner is StatusEffect statusEffect))
            {
                return false;
            }

            if (!BaseScriptingToken.TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> remainingArguments))
            {
                return false;
            }

            if (remainingArguments.Count != 0)
            {
                return false;
            }

            scriptingTokenText = new List<string>()
            {
                "[SETTARGET: SELF]",
                $"[REMOVESTATUSEFFECTSTACKS: {output.GetScriptingTokenText()} {statusEffect.Id}]"
            };
            return true;
        }
    }
}