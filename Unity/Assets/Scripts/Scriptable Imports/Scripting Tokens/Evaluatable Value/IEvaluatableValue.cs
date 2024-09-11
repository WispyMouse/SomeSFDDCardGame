namespace SFDDCards.ScriptingTokens.EvaluatableValues
{

    public interface IEvaluatableValue<T>
    {
        bool TryEvaluateValue(CentralGameStateController gameStatecontroller, out T evaluatedValue);
        string DescribeEvaluation();
    }
}