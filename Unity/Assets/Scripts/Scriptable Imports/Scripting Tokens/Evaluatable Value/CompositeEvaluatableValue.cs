using System.Collections.Generic;
using System.Text;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class CompositeEvaluatableValue<T> : IEvaluatableValue<T>
    {
        public List<IEvaluatableValue<T>> CompositeComponents = new List<IEvaluatableValue<T>>();

        public bool TryEvaluateValue(CentralGameStateController gameStatecontroller, out T evaluatedValue)
        {
            // TODO: Actually composite things! This only takes the first thing, ignoring everything else.
            if (!this.CompositeComponents[0].TryEvaluateValue(gameStatecontroller, out evaluatedValue))
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
    }
}