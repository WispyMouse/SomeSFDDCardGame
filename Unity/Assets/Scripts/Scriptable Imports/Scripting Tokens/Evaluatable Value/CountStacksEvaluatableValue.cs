using System.Collections.Generic;
using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;
using SFDDCards.Evaluation.Conceptual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class CountStacksEvaluatableValue : IEvaluatableValue<int>
    {
        public readonly string StacksToCount;
        public readonly string CountOn;

        public CountStacksEvaluatableValue(string stacksToCount, string countOn)
        {
            this.StacksToCount = stacksToCount;
            this.CountOn = countOn;
        }

        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out int evaluatedValue)
        {
            if (CountOn.ToLower() == "self")
            {
                evaluatedValue = currentBuilder.User.CountStacks(this.StacksToCount);
                return true;
            }

            if (currentBuilder.Target == null)
            {
                evaluatedValue = 0;
                return false;
            }

            evaluatedValue = currentBuilder.Target.CountStacks(this.StacksToCount);
            return true;
        }

        public string DescribeEvaluation(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder)
        {
            if (this.TryEvaluateValue(campaignContext, currentBuilder, out int evaluatedValue))
            {
                return $"{this.DescribeEvaluation()} ({evaluatedValue})";
            }
            return this.DescribeEvaluation();
        }

        public string DescribeEvaluation()
        {
            return this.DescribeEvaluation(this);
        }

        public static bool TryGetCountStacksEvaluatableValue(string argument, out CountStacksEvaluatableValue output, bool allowNameMatch)
        {
            // Check to see if this is counting self
            Match regexMatch = Regex.Match(argument, @"COUNTSTACKSSELF_(\w+)");
            string stackId;

            if (regexMatch.Success)
            {
                stackId = regexMatch.Groups[1].Value;
                output = new CountStacksEvaluatableValue(stackId, "self");
                return true;
            }

            regexMatch = Regex.Match(argument, @"COUNTSTACKS_(\w+)");

            if (regexMatch.Success)
            {
                stackId = regexMatch.Groups[1].Value;
                output = new CountStacksEvaluatableValue(stackId, "target");
                return true;
            }

            if (allowNameMatch && StatusEffectDatabase.TryGetStatusEffectById(argument, out StatusEffect statusEffect))
            {
                output = new CountStacksEvaluatableValue(argument, "target");
                return true;
            }

            output = null;
            return false;
        }

        public string GetScriptingTokenText()
        {
            if (this.CountOn == "self")
            {
                return $"COUNTSTACKSSELF_{this.StacksToCount}";
            }

            return $"COUNTSTACKS_{this.StacksToCount}";
        }

        public string DescribeEvaluation(IEvaluatableValue<int> topValue)
        {
            if (topValue == this)
            {
                return $"1 x {StatusEffectDatabase.GetModel(this.StacksToCount).Name}";
            }

            return $"{StatusEffectDatabase.GetModel(this.StacksToCount).Name}";
        }
    }
}