namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    using SFDDCards.Evaluation.Actual;

    public class OriginalTargetEvaluatableValue : CombatantTargetEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "Original Target";
        }

        public override string GetScriptingTokenText()
        {
            return "original";
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue)
        {
            evaluatedValue = currentBuilder.OriginalTarget;
            return true;
        }
    }
}