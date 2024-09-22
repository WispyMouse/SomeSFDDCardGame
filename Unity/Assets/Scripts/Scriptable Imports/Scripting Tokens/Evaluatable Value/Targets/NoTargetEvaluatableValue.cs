namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    using SFDDCards.Evaluation.Actual;

    public class NoTargetEvaluatableValue : CombatantTargetEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "No Target";
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue)
        {
            evaluatedValue = null;
            return true;
        }
    }
}