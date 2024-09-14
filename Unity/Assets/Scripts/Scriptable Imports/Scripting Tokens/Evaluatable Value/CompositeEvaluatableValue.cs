using System.Collections.Generic;
using System.Text;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class CompositeEvaluatableValue<T> : IEvaluatableValue<T>
    {
        public List<IEvaluatableValue<T>> CompositeComponents = new List<IEvaluatableValue<T>>();

        public bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out T evaluatedValue)
        {
            // TODO: Actually composite things! This only takes the first thing, ignoring everything else.
            if (!this.CompositeComponents[0].TryEvaluateValue(campaignContext, currentBuilder, out evaluatedValue))
            {
                return false;
            }

            return true;
        }

        public string DescribeEvaluation()
        {
            StringBuilder builtString = new StringBuilder();

            foreach (IEvaluatableValue<T> compositeValue in this.CompositeComponents)
            {
                builtString.Append(compositeValue.DescribeEvaluation());
            }

            return builtString.ToString();
        }

        /// <summary>
        /// If this composite has only one element, assigns the ref value to it.
        /// </summary>
        /// <param name="component">Either the only component in this object, or this object.</param>
        public void AttemptAssignSingleComponent(ref IEvaluatableValue<T> component)
        {
            if (this.CompositeComponents.Count == 1)
            {
                component = this.CompositeComponents[0];
                return;
            }

            component = this;
        }
    }
}