namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    using SFDDCards.Evaluation.Actual;

    public class NoTargetEvaluatableValue : CombatantTargetEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "none";
        }

        public override string GetScriptingTokenText()
        {
            throw new System.NotImplementedException();
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue)
        {
            evaluatedValue = null;
            return true;
        }
    }
}