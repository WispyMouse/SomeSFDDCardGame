using SFDDCards.Evaluation.Actual;
using System.Collections.Generic;
using System.Text;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class CompositeEvaluatableValue : IEvaluatableValue<int>
    {
        public enum CommonMath
        {
            FirstElement,
            Plus,
            Minus,
            Divide,
            Multiply
        }

        public struct CompositeNext
        {
            public IEvaluatableValue<int> ThisValue;
            public CommonMath RelationToPrevious;
        }

        public List<CompositeNext> CompositeComponents = new List<CompositeNext>();

        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out int evaluatedValue)
        {
            int runningValue = 0;

            foreach (CompositeNext compositeValue in this.CompositeComponents)
            {
                if (!compositeValue.ThisValue.TryEvaluateValue(campaignContext, currentBuilder, out int currentValue))
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
                }
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
                component = new ConstantEvaluatableValue<int>(total);
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
                }
                builtString.Append(compositeValue.ThisValue.GetScriptingTokenText());
            }

            return builtString.ToString();
        }

        public bool GetEntireCompositeIsConstants()
        {
            foreach (CompositeNext composite in this.CompositeComponents)
            {
                if (composite.ThisValue is ConstantEvaluatableValue<int>)
                {
                    continue;
                }

                if (composite.ThisValue is NegatorEvaluatorValue negater && negater.ToNegate is ConstantEvaluatableValue<int>)
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
                }
                builtString.Append(compositeValue.ThisValue.DescribeEvaluation(topValue));
            }

            return builtString.ToString();
        }
    }
}