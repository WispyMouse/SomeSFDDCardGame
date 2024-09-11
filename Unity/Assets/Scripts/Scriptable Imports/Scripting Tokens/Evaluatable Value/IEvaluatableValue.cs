namespace SFDDCards.ScriptingTokens.EvaluatableValues
{

    public interface IEvaluatableValue<T>
    {
        bool TryEvaluateValue(CentralGameStateController gameStatecontroller, TokenEvaluatorBuilder currentEvaluator, out T evaluatedValue);
        string DescribeEvaluation();
    }
}