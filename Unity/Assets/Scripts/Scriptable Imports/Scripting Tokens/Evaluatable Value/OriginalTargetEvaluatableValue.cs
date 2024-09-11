namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class OriginalTargetEvaluatableValue : CombatantTargetEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "Original Target";
        }

        public override bool TryEvaluateValue(CentralGameStateController gameStatecontroller, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue)
        {
            evaluatedValue = currentBuilder.OriginalTarget;
            return true;
        }
    }
}