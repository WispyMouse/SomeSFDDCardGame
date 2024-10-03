using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class NegatorEvaluatorValue : IEvaluatableValue<int>
    {
        public readonly IEvaluatableValue<int> ToNegate;

        public NegatorEvaluatorValue(IEvaluatableValue<int> toNegate)
        {
            this.ToNegate = toNegate;
        }

        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out int evaluatedValue)
        {
            if (!this.ToNegate.TryEvaluateValue(campaignContext, currentBuilder, out evaluatedValue))
            {
                return false;
            }

            evaluatedValue = evaluatedValue * -1;
            return true;
        }

        public string DescribeEvaluation(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder)
        {
            if (this.TryEvaluateValue(campaignContext, currentBuilder, out int evaluatedValue))
            {
                return $"-{evaluatedValue}";
            }
            return this.DescribeEvaluation();
        }

        public string DescribeEvaluation()
        {
            return "-";
        }

        public static bool TryGetConstantEvaluatableValue(string argument, out NegatorEvaluatorValue output)
        {
            output = null;
            return false;
        }

        public string GetScriptingTokenText()
        {
            return $"-{this.ToNegate.GetScriptingTokenText()}";
        }

        public string DescribeEvaluation(IEvaluatableValue<int> topValue)
        {
            return this.DescribeEvaluation();
        }
    }
}