using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;
using SFDDCards.ImportModels;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{

    public class ConstantNumericEvaluatableValue : ConstantEvaluatableValue<int>, INumericEvaluatableValue
    {
        public decimal DecimalValue;

        public ConstantNumericEvaluatableValue(decimal inputValue) : base((int)inputValue)
        {
            this.DecimalValue = inputValue;
        }

        public bool TryEvaluateDecimalValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentEvaluator, out decimal evaluatedValue)
        {
            evaluatedValue = this.DecimalValue;
            return true;
        }

        public static bool TryGetConstantNumericEvaluatableValue(string argument, out ConstantNumericEvaluatableValue output)
        {
            // ~ indicates a range, which is not constant.
            if (argument.Contains("~"))
            {
                output = null;
                return false;
            }

            if (Regex.IsMatch(argument, @"^\-?\d+(\.\d+)?$"))
            {
                if (decimal.TryParse(argument, out decimal result))
                {
                    output = new ConstantNumericEvaluatableValue(result);
                    return true;
                }
                else
                {
                    output = null;
                    return false;
                }
            }

            output = null;
            return false;
        }
    }
}