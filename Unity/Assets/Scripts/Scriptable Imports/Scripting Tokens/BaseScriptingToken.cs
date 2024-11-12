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
        const string ScriptingTokenEnder = "]";
        const string ArgumentSeparatorFromIdentifier = ":";

        public bool SkipDescribingMe { get; protected set; } = false;

        public abstract string ScriptingTokenIdentifier { get; }

        public abstract void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder);

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            match = null;

            // Compare the token string by extracting the [ and ] so we can evaluate it without those
            string withoutStarters = tokenString.ToLower();
            if (withoutStarters.StartsWith(ScriptingTokenStarter) && withoutStarters.EndsWith(ScriptingTokenEnder))
            {
                withoutStarters = withoutStarters.Substring(ScriptingTokenStarter.Length, withoutStarters.Length - ScriptingTokenStarter.Length - ScriptingTokenEnder.Length);
            }

            if (withoutStarters == this.ScriptingTokenIdentifier.ToLower())
            {
                // Is this just exactly our identifier, with nothing else?
                // If so, enter this if statement so that it can be parsed later down
            }
            else if (!withoutStarters.Contains(ArgumentSeparatorFromIdentifier))
            {
                // Does this contain the argument separator? If not, we've already disqualified this
                return false;
            }
            else
            {
                // Split before the separator and compare
                string beforeSeparator = withoutStarters.Substring(0, withoutStarters.IndexOf(ArgumentSeparatorFromIdentifier));

                // Now is it a match? If not, exit
                if (beforeSeparator != this.ScriptingTokenIdentifier.ToLower())
                {
                    return false;
                }
            }

            // Now that we've made it here, we must have matched the symbol at least
            // So now try to derive arguments and process if those are a hit
            
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

        public static bool TryDeriveArgumentsFromScriptingToken(string tokenString, out List<string> results)
        {
            // If the token string doesn't contain any of the : separators, then there are no arguments
            if (tokenString.StartsWith(ScriptingTokenStarter) && !tokenString.Contains(ArgumentSeparatorFromIdentifier))
            {
                results = new List<string>();
                return true;
            }

            Match nonIdentifierItems;
            if (tokenString.StartsWith(ScriptingTokenStarter))
            {
                nonIdentifierItems = Regex.Match(tokenString, @"^\[.*?\:(.*?)\]");
            }
            else
            {
                nonIdentifierItems = Regex.Match(tokenString, "(.*)");
            }

            if (!nonIdentifierItems.Success)
            {
                results = new List<string>();
                return false;
            }

            results = new List<string>(nonIdentifierItems.Groups[1].Value.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries));
            return true;
        }

        public static bool TryGetIntegerEvaluatableFromString(string arguments, out IEvaluatableValue<int> output, bool allowNameMatch = false)
        {
            return TryGetIntegerEvaluatableFromStrings(new List<string>() { arguments }, out output, out _, allowNameMatch);
        }

        public static bool TryGetIntegerEvaluatableFromStrings(List<string> arguments, out IEvaluatableValue<int> output, out List<string> remainingStrings, bool allowNameMatch  = false)
        {
            remainingStrings = new List<string>();
            CompositeEvaluatableValue compositeEvaluatable = new CompositeEvaluatableValue();
            output = null;

            for (int ii = 0; ii < arguments.Count; ii++)
            {
                string currentArgument = arguments[ii];

                CompositeEvaluatableValue.CommonMath currentMath = CompositeEvaluatableValue.CommonMath.FirstElement;
                StringBuilder currentBuilder = new StringBuilder();
                for (int jj = 0; jj < currentArgument.Length; jj++)
                {
                    bool hitMath = false;
                    CompositeEvaluatableValue.CommonMath nextMath = currentMath;

                    switch (currentArgument[jj])
                    {
                        case '+':
                            nextMath = CompositeEvaluatableValue.CommonMath.Plus;
                            hitMath = true;
                            break;
                        case '/':
                            nextMath = CompositeEvaluatableValue.CommonMath.Divide;
                            hitMath = true;
                            break;
                        case '-':
                            nextMath = CompositeEvaluatableValue.CommonMath.Minus;
                            hitMath = true;
                            break;
                        case '*':
                            nextMath = CompositeEvaluatableValue.CommonMath.Multiply;
                            hitMath = true;
                            break;
                        case '~':
                            nextMath = CompositeEvaluatableValue.CommonMath.Range;
                            hitMath = true;
                            break;
                        default:
                            currentBuilder.Append(currentArgument[jj]);
                            break;
                    }

                    // Include - s if it is at the start of a composite
                    // but also don't processm math yet
                    if (compositeEvaluatable.CompositeComponents.Count == 0 && currentMath == CompositeEvaluatableValue.CommonMath.FirstElement && nextMath == CompositeEvaluatableValue.CommonMath.Minus && jj == 0)
                    {
                        currentBuilder.Append(currentArgument[jj]);
                    }

                    if ((hitMath && jj > 0) || jj == currentArgument.Length - 1)
                    {
                        string thisPiece = currentBuilder.ToString();
                        currentBuilder.Clear();

                        bool successfulParse = false;
                        if (ConstantNumericEvaluatableValue.TryGetConstantNumericEvaluatableValue(thisPiece, out ConstantNumericEvaluatableValue outputCEVI))
                        {
                            compositeEvaluatable.CompositeComponents.Add(new CompositeEvaluatableValue.CompositeNext() { RelationToPrevious = currentMath, ThisValue = outputCEVI });
                            successfulParse = true;
                        }
                        else if (CountStacksEvaluatableValue.TryGetCountStacksEvaluatableValue(thisPiece, out CountStacksEvaluatableValue outputCSEV, allowNameMatch))
                        {
                            compositeEvaluatable.CompositeComponents.Add(new CompositeEvaluatableValue.CompositeNext() { RelationToPrevious = currentMath, ThisValue = outputCSEV });
                            successfulParse = true;
                        }
                        else if (CountElementEvaluatableValue.TryGetCountElementalEvaluatableValue(thisPiece, out CountElementEvaluatableValue outputCEEV, allowNameMatch))
                        {
                            compositeEvaluatable.CompositeComponents.Add(new CompositeEvaluatableValue.CompositeNext() { RelationToPrevious = currentMath, ThisValue = outputCEEV });
                            successfulParse = true;
                        }
                        else if (CountCurrencyEvaluatableValue.TryGetCountCurrencyEvaluatableValue(thisPiece, out CountCurrencyEvaluatableValue outputCCEV, allowNameMatch))
                        {
                            compositeEvaluatable.CompositeComponents.Add(new CompositeEvaluatableValue.CompositeNext() { RelationToPrevious = currentMath, ThisValue = outputCCEV });
                            successfulParse = true;
                        }
                        else if (HealthEvaluatableValue.TryGetHealthEvaluatableValue(thisPiece, out HealthEvaluatableValue outputHEV))
                        {
                            compositeEvaluatable.CompositeComponents.Add(new CompositeEvaluatableValue.CompositeNext() { RelationToPrevious = currentMath, ThisValue = outputHEV });
                            successfulParse = true;
                        }

                        if (!successfulParse || jj == currentArgument.Length - 1)
                        {
                            // We've hit something that isn't a constant, therefore end here if we've *started* to capture anything
                            // If we haven't hit anything captured yet, perhaps because the constant is printed on the right of the text,
                            // then continue, putting the previous things into the remaining arguments list

                            if (compositeEvaluatable.CompositeComponents.Count == 0)
                            {
                                // Add any previous arguments to the remaining strings list
                                for (int kk = 0; kk <= ii; kk++)
                                {
                                    remainingStrings.Add(arguments[kk]);
                                }
                                break;
                            }
                            else
                            {
                                // Add any remaining arguments to the remaining strings list
                                for (int kk = ii + 1; kk < arguments.Count; kk++)
                                {
                                    remainingStrings.Add(arguments[kk]);
                                }

                                compositeEvaluatable.AttemptAssignSingleComponent(ref output);
                                return true;
                            }
                        }
                    }

                    currentMath = nextMath;
                }
            }

            if (compositeEvaluatable.CompositeComponents.Count == 0)
            {
                return false;
            }

            compositeEvaluatable.AttemptAssignSingleComponent(ref output);
            return true;
        }

        protected abstract bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken);

        protected void EnsureTarget(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            if (tokenBuilder.Target == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"This effect requires a target to be specified before it can be applied to a token builder. Please specify a target.", GlobalUpdateUX.LogType.RuntimeError);
            }
        }
    }
}