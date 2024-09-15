using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class HealthEvaluatableValue : IEvaluatableValue<int>
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

            if (!currentBuilder.Target.TryEvaluateValue(campaignContext, currentBuilder, out ICombatantTarget target))
            {
                evaluatedValue = 0;
                return false;
            }

            evaluatedValue = target.GetTotalHealth();
            return true;
        }

        public string DescribeEvaluation()
        {
            return $"current health of {this.TargetToAssess}";
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
    }
}