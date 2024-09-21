namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public abstract class BaseScriptingToken : IScriptingToken
    {
        const string ScriptingTokenStarter = "[";
        const string ArgumentSeparatorFromIdentifier = ":";

        public abstract string ScriptingTokenIdentifier { get; }

        public abstract void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder);

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

        public static bool TryGetIntegerEvaluatableFromStrings(List<string> arguments, out IEvaluatableValue<int> output, out List<string> remainingStrings)
        {
            remainingStrings = new List<string>();
            CompositeEvaluatableValue<int> compositeEvaluatable = new CompositeEvaluatableValue<int>();
            output = null;

            for (int ii = 0; ii < arguments.Count; ii++)
            {
                string currentArgument = arguments[ii];

                if (ConstantEvaluatableValue<int>.TryGetConstantEvaluatableValue(currentArgument, out ConstantEvaluatableValue<int> outputCEVI))
                {
                    compositeEvaluatable.CompositeComponents.Add(outputCEVI);
                    continue;
                }
                else if (CountStacksEvaluatableValue.TryGetCountStacksEvaluatableValue(currentArgument, out CountStacksEvaluatableValue outputCSEV))
                {
                    compositeEvaluatable.CompositeComponents.Add(outputCSEV);
                    continue;
                }
                else if (CountElementEvaluatableValue.TryGetCountElementalEvaluatableValue(currentArgument, out CountElementEvaluatableValue outputCEEV))
                {
                    compositeEvaluatable.CompositeComponents.Add(outputCEEV);
                    continue;
                }
                else if (HealthEvaluatableValue.TryGetHealthEvaluatableValue(currentArgument, out HealthEvaluatableValue outputHEV))
                {
                    compositeEvaluatable.CompositeComponents.Add(outputHEV);
                    continue;
                }

                remainingStrings.Add(currentArgument);
            }

            if (compositeEvaluatable.CompositeComponents.Count == 0)
            {
                return false;
            }

            compositeEvaluatable.AttemptAssignSingleComponent(ref output);
            return true;
        }

        protected abstract bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken);

        public abstract bool RequiresTarget();
    }
}