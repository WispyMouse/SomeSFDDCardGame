namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public abstract class AliasScriptingToken
    {
        const string ScriptingTokenStarter = "[";

        public abstract string ScriptingTokenIdentifier { get; }

        public bool GetTokensIfMatch(string tokenString, IEffectOwner owner, out List<string> scriptingTokenText)
        {
            scriptingTokenText = null;

            if (!tokenString.StartsWith(ScriptingTokenStarter + this.ScriptingTokenIdentifier, System.StringComparison.InvariantCultureIgnoreCase))
            {
                // Doesn't start with the indicator, can't be this token.
                return false;
            }

            if (!BaseScriptingToken.TryDeriveArgumentsFromScriptingToken(tokenString, out List<string> resultingArguments))
            {
                // If there was an error in parsing the tokenString, then it can't be this token
                return false;
            }

            if (!TryGetTokenWithArguments(resultingArguments, owner, out scriptingTokenText))
            {
                // If the token can't be generated with the provided arguments, skip this token
                return false;
            }

            return true;
        }

        protected abstract bool TryGetTokenWithArguments(List<string> arguments, IEffectOwner owner, out List<string> scriptingTokenText);
    }
}