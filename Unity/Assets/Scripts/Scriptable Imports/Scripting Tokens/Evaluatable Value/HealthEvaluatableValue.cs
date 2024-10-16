using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class HealthEvaluatableValue : INumericEvaluatableValue
    {
        public readonly string TargetToAssess;

        public HealthEvaluatableValue(string targetToAssess)
        {
            this.TargetToAssess = targetToAssess;
        }

        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out int evaluatedValue)
        {
            if (this.TargetToAssess == "self")
            {
                evaluatedValue = currentBuilder.User.GetTotalHealth();
                return true;
            }

            if (currentBuilder.Target == null)
            {
                evaluatedValue = 0;
                return false;
            }

            evaluatedValue = currentBuilder.Target.GetTotalHealth();
            return true;
        }

        public bool TryEvaluateDecimalValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out decimal evaluatedValue)
        {
            if (this.TargetToAssess == "self")
            {
                evaluatedValue = currentBuilder.User.GetTotalHealth();
                return true;
            }

            if (currentBuilder.Target == null)
            {
                evaluatedValue = 0;
                return false;
            }

            evaluatedValue = currentBuilder.Target.GetTotalHealth();
            return true;
        }

        public string DescribeEvaluation(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder)
        {
            if (this.TryEvaluateValue(campaignContext, currentBuilder, out int evaluatedValue))
            {
                return $"{this.DescribeEvaluation()} ({evaluatedValue})";
            }
            return this.DescribeEvaluation();
        }

        public string DescribeEvaluation()
        {
            if (this.TargetToAssess.ToLower() == "target")
            {
                return "target's health";
            }
            else if (this.TargetToAssess.ToLower() == "self")
            {
                return "own health";
            }

            return $"{this.TargetToAssess} health";
        }

        public static bool TryGetHealthEvaluatableValue(string argument, out HealthEvaluatableValue output)
        {
            Match regexMatch = Regex.Match(argument, @"SELFHEALTH");

            if (regexMatch.Success)
            {
                output = new HealthEvaluatableValue("self");
                return true;
            }

            regexMatch = Regex.Match(argument, @"TARGETHEALTH");

            if (!regexMatch.Success)
            {
                output = null;
                return false;
            }

            output = new HealthEvaluatableValue("target");
            return true;
        }

        public string GetScriptingTokenText()
        {
            return $"{this.TargetToAssess}HEALTH";
        }

        public string DescribeEvaluation(IEvaluatableValue<int> topValue)
        {
            return this.DescribeEvaluation();
        }
    }
}