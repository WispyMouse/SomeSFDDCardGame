namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class NoTargetEvaluatableValue : CombatantTargetEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "No Target";
        }

        public override bool TryEvaluateValue(CentralGameStateController gameStatecontroller, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue)
        {
            evaluatedValue = null;
            return true;
        }
    }
}