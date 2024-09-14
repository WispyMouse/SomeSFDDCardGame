namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class ConstantEvaluatableValue<T> : IEvaluatableValue<T>
    {
        public readonly T ConstantValue;

        public ConstantEvaluatableValue(T inputValue)
        {
            this.ConstantValue = inputValue;
        }

        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out T evaluatedValue)
        {
            evaluatedValue = this.ConstantValue;
            return true;
        }

        public string DescribeEvaluation()
        {
            return this.ConstantValue.ToString();
        }
    }
}