namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class SpecificTargetEvaluatableValue : CombatantTargetEvaluatableValue
    {
        public ICombatantTarget Target { get; set; }

        public SpecificTargetEvaluatableValue(ICombatantTarget toTarget)
        {
            this.Target = toTarget;
        }

        public override string DescribeEvaluation()
        {
            return this.Target.Name;
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue)
        {
            evaluatedValue = this.Target;
            return true;
        }
    }
}