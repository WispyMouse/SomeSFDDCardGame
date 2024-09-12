namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class SelfTargetEvaluatableValue : CombatantTargetEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "Self";
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue)
        {
            evaluatedValue = currentBuilder.User;
            return true;
        }
    }
}