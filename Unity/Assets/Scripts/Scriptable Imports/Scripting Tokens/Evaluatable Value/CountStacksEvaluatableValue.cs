using System.Text.RegularExpressions;

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
            if (this.CountOn == "self" && currentBuilder.User != null)
            {
                evaluatedValue = currentBuilder.User.CountStacks(this.StacksToCount);
            }

            if (currentBuilder.Target == null || !currentBuilder.Target.TryEvaluateValue(campaignContext, currentBuilder, out ICombatantTarget target))
            {
                evaluatedValue = 0;
                return false;
            }

            evaluatedValue = target.CountStacks(this.StacksToCount);
            return true;
        }

        public string DescribeEvaluation()
        {
            return $"number of {StacksToCount}";
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
    }
}