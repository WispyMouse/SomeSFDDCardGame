using SFDDCards.Evaluation.Actual;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public abstract class CardsEvaluatableValue : IEvaluatableValue<List<Card>>
    {
        public const string HandZoneId = "hand";
        public const string DiscardZoneId = "discard";
        public const string ExileZoneId = "exile";
        public const string DeckZoneId = "deck";

        public abstract int RepresentingNumberOfCards(DeltaEntry toApplyTo);

        public abstract string DescribeEvaluation();

        public abstract string GetScriptingTokenText();

        public abstract bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentEvaluator, out List<Card> evaluatedValue);

        public static CardsEvaluatableValue GetEvaluatable(string zone, IEvaluatableValue<int> number)
        {
            if (zone.ToLower() == HandZoneId)
            {
                return new HandCardsEvaluatableValue();
            }
            else if (zone.ToLower() == DiscardZoneId)
            {
                return new DiscardCardsEvaluatableValue();
            }
            else if (zone.ToLower() == ExileZoneId)
            {
                return new ExileCardsEvaluatableValue();
            }
            else if (zone.ToLower() == DeckZoneId)
            {
                return new DeckCardsEvaluatableValue() { TopCardsCount = number };
            }

            GlobalUpdateUX.LogTextEvent.Invoke($"Failed to determine card evaluatable for zone '{zone}'.", GlobalUpdateUX.LogType.RuntimeError);

            return null;
        }

        public string DescribeEvaluation(IEvaluatableValue<List<Card>> topValue)
        {
            return this.DescribeEvaluation();
        }
    }
}