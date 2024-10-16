namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class ClearStacksScriptingToken : AliasScriptingToken
    {
        public override string ScriptingTokenIdentifier => "CLEARSTACKS";

        protected override bool TryGetTokenWithArguments(List<string> arguments, IEffectOwner owner, out List<string> scriptingTokenText)
        {
            scriptingTokenText = null;

            if (owner == null || !(owner is StatusEffect statusEffect))
            {
                return false;
            }

            // If there are any arguments, this isn't applied
            if (arguments.Count > 0)
            {
                return false;
            }

            scriptingTokenText = new List<string>()
            {
                "[SETTARGET: SELF]",
                $"[SETSTATUSEFFECTSTACKS: {owner.Id} 0]"
            };
            return true;
        }
    }
}