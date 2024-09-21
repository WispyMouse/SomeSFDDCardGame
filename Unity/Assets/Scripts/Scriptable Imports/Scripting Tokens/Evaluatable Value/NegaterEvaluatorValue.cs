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

        public string DescribeEvaluation()
        {
            return "-";
        }

        public static bool TryGetConstantEvaluatableValue(string argument, out NegatorEvaluatorValue output)
        {
            output = null;
            return false;
        }
    }
}