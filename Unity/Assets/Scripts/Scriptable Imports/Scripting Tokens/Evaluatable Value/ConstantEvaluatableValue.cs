using SFDDCards.Evaluation.Actual;
using System.Text.RegularExpressions;

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

        public static bool TryGetConstantEvaluatableValue(string argument, out ConstantEvaluatableValue<int> output)
        {
            if (Regex.IsMatch(argument, @"^\-?\d+$"))
            {
                if (int.TryParse(argument, out int result))
                {
                    output = new ConstantEvaluatableValue<int>(result);
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

        public string GetScriptingTokenText()
        {
            if (ConstantValue is int intvalue)
            {
                return intvalue.ToString();
            }

            GlobalUpdateUX.LogTextEvent.Invoke($"Asked to re-parse a constant value other than an int.", GlobalUpdateUX.LogType.RuntimeError);
            return string.Empty;
        }
    }
}