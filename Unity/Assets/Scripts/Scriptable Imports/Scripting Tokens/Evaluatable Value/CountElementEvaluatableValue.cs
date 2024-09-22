using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class CountElementEvaluatableValue : IEvaluatableValue<int>
    {
        public readonly string ElementToCount;

        public CountElementEvaluatableValue(string elementToCount)
        {
            this.ElementToCount = elementToCount;
        }

        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out int evaluatedValue)
        {
            if (!campaignContext.CurrentCombatContext.ElementResourceCounts.TryGetValue(ElementDatabase.GetElement(this.ElementToCount), out evaluatedValue))
            {
                evaluatedValue = 0;
            }

            return true;
        }

        public string DescribeEvaluation()
        {
            return $"amount of {ElementToCount}";
        }

        public static bool TryGetCountElementalEvaluatableValue(string argument, out CountElementEvaluatableValue output)
        {
            Match regexMatch = Regex.Match(argument, @"COUNTELEMENT_(\w+)");

            if (!regexMatch.Success)
            {
                output = null;
                return false;
            }

            string stackId = regexMatch.Groups[1].Value;
            output = new CountElementEvaluatableValue(stackId);
            return true;
        }

        public string GetScriptingTokenText()
        {
            return $"COUNTELEMENT_{this.ElementToCount}";
        }
    }
}