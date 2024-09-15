using SFDDCards.ScriptingTokens.EvaluatableValues;

namespace SFDDCards
{
    public struct ElementResourceChange
    {
        public Element Element;
        public IEvaluatableValue<int> GainOrLoss;
    }
}
