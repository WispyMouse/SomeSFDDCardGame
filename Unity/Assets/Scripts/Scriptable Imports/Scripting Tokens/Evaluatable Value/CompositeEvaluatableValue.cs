using SFDDCards.Evaluation.Actual;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class CompositeEvaluatableValue : INumericEvaluatableValue
    {
        public enum CommonMath
        {
            FirstElement,
            Plus,
            Minus,
            Divide,
            Multiply,
            Range
        }

        public struct CompositeNext
        {
            public INumericEvaluatableValue ThisValue;
            public CommonMath RelationToPrevious;
        }

        public List<CompositeNext> CompositeComponents = new List<CompositeNext>();


        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out int evaluatedValue)
        {
            if (TryEvaluateDecimalValue(campaignContext, currentBuilder, out decimal evaluatedDecimalValue))
            {
                evaluatedValue = Mathf.RoundToInt((float)evaluatedDecimalValue);
                return true;
            }

            evaluatedValue = 0;
            return false;
        }

        public bool TryEvaluateDecimalValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out decimal evaluatedValue)
        {
            decimal runningValue = 0;

            // Range is indicated by a ~
            // It is computed in a Composite by comparing everything to the right of a range indicator to the things on the left
            // if another range indicator is found, or the end of a composite is hit, submit it
            decimal? previousRangeSet = null;

            foreach (CompositeNext compositeValue in this.CompositeComponents)
            {
                if (!compositeValue.ThisValue.TryEvaluateDecimalValue(campaignContext, currentBuilder, out decimal currentValue))
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to evaluate inner value for composite.", GlobalUpdateUX.LogType.RuntimeError);
                }

                switch (compositeValue.RelationToPrevious)
                {
                    case CommonMath.FirstElement:
                        runningValue = currentValue;
                        break;
                    case CommonMath.Plus:
                        runningValue += currentValue;
                        break;
                    case CommonMath.Minus:
                        runningValue -= currentValue;
                        break;
                    case CommonMath.Divide:
                        runningValue /= currentValue;
                        break;
                    case CommonMath.Multiply:
                        runningValue *= currentValue;
                        break;
                    case CommonMath.Range:
                        if (previousRangeSet.HasValue)
                        {
                            runningValue = (decimal)Random.Range(Mathf.Min((float)runningValue, (float)previousRangeSet.Value), Mathf.Max((float)runningValue, (float)previousRangeSet.Value));
                            previousRangeSet = currentValue;
                        }
                        else
                        {
                            previousRangeSet = runningValue;
                            runningValue = currentValue;
                        }
                        break;
                }
            }

            if (previousRangeSet.HasValue)
            {
                runningValue = (decimal)Random.Range(Mathf.Min((float)runningValue, (float)previousRangeSet.Value), Mathf.Max((float)runningValue, (float)previousRangeSet.Value));
            }

            evaluatedValue = runningValue;
            return true;
        }

        public string DescribeEvaluation()
        {
            return this.DescribeEvaluation(this);
        }

        /// <summary>
        /// If this composite has only one element, assigns the ref value to it.
        /// </summary>
        /// <param name="component">Either the only component in this object, or this object.</param>
        public void AttemptAssignSingleComponent(ref IEvaluatableValue<int> component)
        {
            if (this.CompositeComponents.Count == 1)
            {
                component = this.CompositeComponents[0].ThisValue;
                return;
            }

            if (this.GetEntireCompositeIsConstants() && this.TryEvaluateValue(null, null, out int total))
            {
                component = new ConstantNumericEvaluatableValue(total);
                return;
            }

            component = this;
        }

        public string GetScriptingTokenText()
        {
            StringBuilder builtString = new StringBuilder();

            foreach (CompositeNext compositeValue in this.CompositeComponents)
            {
                switch (compositeValue.RelationToPrevious)
                {
                    case CommonMath.Plus:
                        builtString.Append("+");
                        break;
                    case CommonMath.Minus:
                        builtString.Append("-");
                        break;
                    case CommonMath.Divide:
                        builtString.Append("/");
                        break;
                    case CommonMath.Multiply:
                        builtString.Append("*");
                        break;
                    case CommonMath.Range:
                        builtString.Append("~");
                        break;
                }
                builtString.Append(compositeValue.ThisValue.GetScriptingTokenText());
            }

            return builtString.ToString();
        }

        public bool GetEntireCompositeIsConstants()
        {
            foreach (CompositeNext composite in this.CompositeComponents)
            {
                if (composite.RelationToPrevious == CommonMath.Range)
                {
                    return false;
                }

                if (composite.ThisValue is ConstantNumericEvaluatableValue)
                {
                    continue;
                }

                if (composite.ThisValue is NegatorEvaluatorValue negater && negater.ToNegate is ConstantNumericEvaluatableValue)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public string DescribeEvaluation(IEvaluatableValue<int> topValue)
        {
            StringBuilder builtString = new StringBuilder();

            foreach (CompositeNext compositeValue in this.CompositeComponents)
            {
                switch (compositeValue.RelationToPrevious)
                {
                    case CommonMath.Plus:
                        builtString.Append(" + ");
                        break;
                    case CommonMath.Minus:
                        builtString.Append(" - ");
                        break;
                    case CommonMath.Divide:
                        builtString.Append(" / ");
                        break;
                    case CommonMath.Multiply:
                        builtString.Append(" x ");
                        break;
                    case CommonMath.Range:
                        builtString.Append(" ~ ");
                        break;
                }
                builtString.Append(compositeValue.ThisValue.DescribeEvaluation(topValue));
            }

            return builtString.ToString();
        }

        public string DescribeEvaluation(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder)
        {
            if (this.TryEvaluateValue(campaignContext, currentBuilder, out int evaluatedValue))
            {
                return $"{this.DescribeEvaluation()} ({evaluatedValue})";
            }
            return this.DescribeEvaluation();
        }
    }
}