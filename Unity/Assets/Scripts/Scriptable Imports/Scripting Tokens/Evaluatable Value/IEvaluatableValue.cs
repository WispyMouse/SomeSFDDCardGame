using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{

    public interface IEvaluatableValue<T>
    {
        bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentEvaluator, out T evaluatedValue);
        string DescribeEvaluation(CampaignContext campaignContext, TokenEvaluatorBuilder currentEvaluator);
        string DescribeEvaluation();
        string DescribeEvaluation(IEvaluatableValue<T> topValue);
        string GetScriptingTokenText();
    }
}