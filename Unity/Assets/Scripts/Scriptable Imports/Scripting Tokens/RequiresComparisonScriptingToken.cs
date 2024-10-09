using SFDDCards.ScriptingTokens.EvaluatableValues;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;
using SFDDCards.Evaluation.Conceptual;

namespace SFDDCards.ScriptingTokens
{
    public class RequiresComparisonScriptingToken : BaseScriptingToken, IRequirement
    {
        public const string GreaterThanOrEqualToAscii = "\u2265";
        public const string LessThanOrEqualToAscii = "\u2264";
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

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Requirements.Add(this);
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

        public static string GetComparisonString(Comparison argument)
        {
            switch (argument)
            {
                case Comparison.NotAComparison:
                default:
                    return string.Empty;
                case Comparison.LessThanOrEqual:
                    return LessThanOrEqualToAscii;
                case Comparison.LessThan:
                    return "<";
                case Comparison.EqualTo:
                    return "=";
                case Comparison.GreaterThan:
                    return ">";
                case Comparison.GreaterThanOrEqual:
                    return GreaterThanOrEqualToAscii;
            }
        }

        public bool MeetsRequirement(TokenEvaluatorBuilder builder, CampaignContext context)
        {
            if (!this.Left.TryEvaluateValue(context, builder, out int leftValue))
            {
                return false;
            }

            if (!this.Right.TryEvaluateValue(context, builder, out int rightValue))
            {
                return false;
            }

            switch (this.ComparisonType)
            {
                case RequiresComparisonScriptingToken.Comparison.LessThan:
                    return leftValue < rightValue;
                case RequiresComparisonScriptingToken.Comparison.LessThanOrEqual:
                    return leftValue <= rightValue;
                case RequiresComparisonScriptingToken.Comparison.EqualTo:
                    return leftValue == rightValue;
                case RequiresComparisonScriptingToken.Comparison.GreaterThan:
                    return leftValue > rightValue;
                case RequiresComparisonScriptingToken.Comparison.GreaterThanOrEqual:
                    return leftValue >= rightValue;
            }

            return false;
        }

        public string DescribeRequirement()
        {
            return $"{this.Left.DescribeEvaluation()} {GetComparisonString(this.ComparisonType)} {this.Right.DescribeEvaluation()}";
        }
    }
}