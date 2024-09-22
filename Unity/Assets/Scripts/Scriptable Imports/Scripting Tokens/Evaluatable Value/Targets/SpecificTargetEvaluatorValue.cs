namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    using SFDDCards.Evaluation.Actual;

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

        public override string GetScriptingTokenText()
        {
            GlobalUpdateUX.LogTextEvent.Invoke($"This is a specific target token. It should not be parsed back into text. This possibility existing shows that this needs to have a different API for determining tokens.", GlobalUpdateUX.LogType.RuntimeError);
            return string.Empty;
        }
    }
}