namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public abstract class BaseScriptingToken : IScriptingToken
    {
        const string ScriptingTokenStarter = "[";
        const string ArgumentSeparatorFromIdentifier = ":";

        public abstract string ScriptingTokenIdentifier { get; }

        public abstract void ApplyToken(TokenEvaluatorBuilder tokenBuilder);

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            match = null;

            if (!tokenString.StartsWith(ScriptingTokenStarter + this.ScriptingTokenIdentifier, System.StringComparison.InvariantCultureIgnoreCase))
            {
                // Doesn't start with the indicator, can't be this token.
                return false;
            }

            if (!TryDeriveArgumentsFromScriptingToken(tokenString, out List<string> resultingArguments))
            {
                // If there was an error in parsing the tokenString, then it can't be this token
                return false;
            }

            if (!TryGetTokenWithArguments(resultingArguments, out match))
            {
                // If the token can't be generated with the provided arguments, skip this token
                return false;
            }

            return true;
        }

        public virtual bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }

        public static bool TryDeriveArgumentsFromScriptingToken(string tokenString, out List<string> results)
        {
            // If the token string doesn't contain any of the : separators, then there are no arguments
            if (!tokenString.Contains(ArgumentSeparatorFromIdentifier))
            {
                results = new List<string>();
                return true;
            }

            Match nonIdentifierItems = Regex.Match(tokenString, @"^\[.*?\:(.*?)\]");
            if (!nonIdentifierItems.Success)
            {
                results = new List<string>();
                return false;
            }

            results = new List<string>(nonIdentifierItems.Groups[1].Value.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries));
            return true;
        }

        public static bool TryGetIntegerEvaluatableFromStrings(List<string> arguments, out IEvaluatableValue<int> output)
        {
            CompositeEvaluatableValue<int> compositeEvaluatable = new CompositeEvaluatableValue<int>();
            output = null;

            for (int ii = 0; ii < arguments.Count; ii++)
            {
                string currentArgument = arguments[ii];

                if (Regex.IsMatch(currentArgument, @"\-?\d+"))
                {
                    if (int.TryParse(currentArgument, out int result))
                    {
                        compositeEvaluatable.CompositeComponents.Add(new ConstantEvaluatableValue<int>(result));
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (compositeEvaluatable.CompositeComponents.Count == 0)
            {
                return false;
            }

            output = compositeEvaluatable;
            return true;
        }

        protected abstract bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken);
    }
}