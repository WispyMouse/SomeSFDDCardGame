using System.Collections.Generic;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class SelfCardEvaluatableValue : CardsEvaluatableValue
    {
        public Card SelfCard;

        public SelfCardEvaluatableValue(Card selfCard)
        {
            this.SelfCard = selfCard;
        }

        public override string DescribeEvaluation()
        {
            return $"this card";
        }

        public override string GetScriptingTokenText()
        {
            return "self";
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out List<Card> evaluatedValue)
        {
            evaluatedValue = new List<Card>() { SelfCard };
            return true;
        }

        public override int RepresentingNumberOfCards(DeltaEntry toApplyTo)
        {
            return 1;
        }

        public override bool Equals(CardsEvaluatableValue other)
        {
            if (other is SpecificCardsEvaluatableValue specific && specific.Cards.Count == 1)
            {
                return this.SelfCard == specific.Cards[0];
            }

            if (!(other is SelfCardEvaluatableValue otherCard))
            {
                return false;
            }

            return this.SelfCard == otherCard.SelfCard;
        }
    }
}