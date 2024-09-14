namespace SFDDCards.ScriptingTokens.EvaluatableValues
{

    public interface IEvaluatableValue<T>
    {
        bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentEvaluator, out T evaluatedValue);
        string DescribeEvaluation();
    }
}