using System.Collections.Generic;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class SpecificCardsEvaluatableValue : CardsEvaluatableValue
    {
        public List<Card> Cards { get; set; }

        public SpecificCardsEvaluatableValue(List<Card> specificCards)
        {
            this.Cards = specificCards;
        }

        public override string DescribeEvaluation()
        {
            return $"{this.Cards.Count} selected cards";
        }

        public override string GetScriptingTokenText()
        {
            GlobalUpdateUX.LogTextEvent.Invoke($"This is a specific target token. It should not be parsed back into text. This possibility existing shows that this needs to have a different API for determining tokens.", GlobalUpdateUX.LogType.RuntimeError);
            return string.Empty;
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out List<Card> evaluatedValue)
        {
            evaluatedValue = this.Cards;
            return true;
        }

        public override int RepresentingNumberOfCards(DeltaEntry toApplyTo)
        {
            return this.Cards.Count;
        }
    }
}