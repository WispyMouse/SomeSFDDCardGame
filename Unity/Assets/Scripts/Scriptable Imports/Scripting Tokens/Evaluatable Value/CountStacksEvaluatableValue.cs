using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class CountStacksEvaluatableValue : IEvaluatableValue<int>
    {
        public readonly string StacksToCount;

        public CountStacksEvaluatableValue(string stacksToCount)
        {
            this.StacksToCount = stacksToCount;
        }

        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out int evaluatedValue)
        {
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

        public static bool TryGetConstantEvaluatableValue(string argument, out CountStacksEvaluatableValue output)
        {
            Match regexMatch = Regex.Match(argument, @"COUNTSTACKS_(\w+)");

            if (!regexMatch.Success)
            {
                output = null;
                return false;
            }

            string stackId = regexMatch.Groups[1].Value;
            output = new CountStacksEvaluatableValue(stackId);
            return true;
        }
    }
}