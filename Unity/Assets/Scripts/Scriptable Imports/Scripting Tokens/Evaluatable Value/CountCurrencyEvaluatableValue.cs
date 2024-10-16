using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;
using SFDDCards.ImportModels;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class CountCurrencyEvaluatableValue : INumericEvaluatableValue
    {
        public readonly string CurrencyToCount;

        public CountCurrencyEvaluatableValue(string currencyToCount)
        {
            this.CurrencyToCount = currencyToCount;
        }

        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out int evaluatedValue)
        {
            evaluatedValue = campaignContext.GetCurrencyCount(CurrencyDatabase.GetModel(this.CurrencyToCount));
            return true;
        }

        public bool TryEvaluateDecimalValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out decimal evaluatedValue)
        {
            bool returnValue = this.TryEvaluateValue(campaignContext, currentBuilder, out int evaluatedIntValue);
            evaluatedValue = evaluatedIntValue;
            return returnValue;
        }

        public string DescribeEvaluation()
        {
            return this.DescribeEvaluation(this);
        }

        public string DescribeEvaluation(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder)
        {
            if (this.TryEvaluateValue(campaignContext, currentBuilder, out int evaluatedValue))
            {
                return $"{this.DescribeEvaluation()} ({evaluatedValue})";
            }
            return this.DescribeEvaluation();
        }

        public static bool TryGetCountCurrencyEvaluatableValue(string argument, out CountCurrencyEvaluatableValue output, bool allowNameMatch)
        {
            Match regexMatch = Regex.Match(argument, @"COUNTCURRENCY_(\w+)");

            if (regexMatch.Success)
            {
                string stackId = regexMatch.Groups[1].Value;
                output = new CountCurrencyEvaluatableValue(stackId);
                return true;
            }

            if (allowNameMatch && CurrencyDatabase.TryGetModel(argument, out CurrencyImport currency))
            {
                output = new CountCurrencyEvaluatableValue(currency.Id);
                return true;
            }

            output = null;
            return false;
        }

        public string GetScriptingTokenText()
        {
            return $"COUNTCURRENCY_{this.CurrencyToCount}";
        }

        public string DescribeEvaluation(IEvaluatableValue<int> topValue)
        {
            CurrencyImport currency = CurrencyDatabase.GetModel(this.CurrencyToCount);

            if (topValue == this)
            {
                return $"1 x {currency.GetNameAndMaybeIcon()}";
            }

            return $"{currency.GetNameAndMaybeIcon()}";
        }
    }
}