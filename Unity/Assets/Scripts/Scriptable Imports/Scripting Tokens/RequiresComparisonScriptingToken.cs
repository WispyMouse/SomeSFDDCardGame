using SFDDCards.ScriptingTokens.EvaluatableValues;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens
{
    public class RequiresComparisonScriptingToken : BaseScriptingToken
    {
        public enum Comparison
        {
            NotAComparison = 0,
            GreaterThan,
            EqualTo,
            LessThan,
            GreaterThanOrEqual,
            LessThanOrEqual
        }

        public IEvaluatableValue<int> Left { get; private set; }
        public IEvaluatableValue<int> Right { get; private set; }
        public Comparison ComparisonType { get; private set; }

        public override string ScriptingTokenIdentifier => "requirescomparison";

        public override void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.RequiresComparisons.Add(this);
        }

        public override bool RequiresTarget()
        {
            return false;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            if (arguments.Count < 3)
            {
                scriptingToken = null;
                return false;
            }

            // We're looking for a left clause, a comparison value, and a right clause
            // First try to split the arguments in to those groups
            List<string> leftArguments = new List<string>();
            Comparison comparisonType = Comparison.NotAComparison;
            for (int ii = 0; ii < arguments.Count; ii++)
            {
                comparisonType = GetComparison(arguments[ii]);
                if (comparisonType != Comparison.NotAComparison)
                {
                    break;
                }

                leftArguments.Add(arguments[ii]);
            }

            if (leftArguments.Count == 0 || leftArguments.Count > arguments.Count - 2)
            {
                scriptingToken = null;
                return false;
            }

            List<string> rightArguments = new List<string>();
            for (int ii = leftArguments.Count + 1; ii < arguments.Count; ii++)
            {
                rightArguments.Add(arguments[ii]);
            }

            if (!TryGetIntegerEvaluatableFromStrings(leftArguments, out IEvaluatableValue<int> leftOutput, out List<string> _))
            {
                return false;
            }

            if (!TryGetIntegerEvaluatableFromStrings(rightArguments, out IEvaluatableValue<int> rightOutput, out List<string> _))
            {
                return false;
            }

            scriptingToken = new RequiresComparisonScriptingToken()
            {
                ComparisonType = comparisonType,
                Left = leftOutput,
                Right = rightOutput
            };

            return true;
        }

        public static Comparison GetComparison(string argument)
        {
            switch (argument)
            {
                case ">":
                    return Comparison.GreaterThan;
                case "<":
                    return Comparison.LessThan;
                case "=":
                    return Comparison.EqualTo;
                case "<=":
                    return Comparison.LessThanOrEqual;
                case ">=":
                    return Comparison.GreaterThanOrEqual;
                default:
                    return Comparison.NotAComparison;
            }
        }
    }
}