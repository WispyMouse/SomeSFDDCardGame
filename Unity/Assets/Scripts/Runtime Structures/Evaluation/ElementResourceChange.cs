using SFDDCards.ScriptingTokens.EvaluatableValues;

namespace SFDDCards
{
    public struct ElementResourceChange
    {
        public Element Element;
        public IEvaluatableValue<int> GainOrLoss;
        public IEvaluatableValue<int> SetValue;

        public ElementResourceChange(Element element, IEvaluatableValue<int> gainOrLoss = null, IEvaluatableValue<int> setValue = null)
        {
            this.Element = element;
            this.GainOrLoss = gainOrLoss;
            this.SetValue = setValue;
        }
    }
}
