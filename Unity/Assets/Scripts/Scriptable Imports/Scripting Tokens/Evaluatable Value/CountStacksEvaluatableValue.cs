using System.Collections.Generic;
using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;
using SFDDCards.Evaluation.Conceptual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class CountStacksEvaluatableValue : IEvaluatableValue<int>
    {
        public readonly string StacksToCount;
        public readonly string CountOn;

        public CountStacksEvaluatableValue(string stacksToCount, string countOn)
        {
            this.StacksToCount = stacksToCount;
            this.CountOn = countOn;
        }

        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out int evaluatedValue)
        {
            if (currentBuilder.Target == null)
            {
                evaluatedValue = 0;
                return false;
            }

            evaluatedValue = currentBuilder.Target.CountStacks(this.StacksToCount);
            return true;
        }

        public string DescribeEvaluation()
        {
            return $"1 x {this.StacksToCount}";
        }

        public static bool TryGetCountStacksEvaluatableValue(string argument, out CountStacksEvaluatableValue output)
        {
            // Check to see if this is counting self
            Match regexMatch = Regex.Match(argument, @"COUNTSTACKSSELF_(\w+)");
            string stackId;

            if (regexMatch.Success)
            {
                stackId = regexMatch.Groups[1].Value;
                output = new CountStacksEvaluatableValue(stackId, "self");
                return true;
            }

            regexMatch = Regex.Match(argument, @"COUNTSTACKS_(\w+)");

            if (!regexMatch.Success)
            {
                output = null;
                return false;
            }

            stackId = regexMatch.Groups[1].Value;
            output = new CountStacksEvaluatableValue(stackId, "target");
            return true;
        }

        public string GetScriptingTokenText()
        {
            if (this.CountOn == "self")
            {
                return $"COUNTSTACKSSELF_{this.StacksToCount}";
            }

            return $"COUNTSTACKS_{this.StacksToCount}";
        }
    }
}