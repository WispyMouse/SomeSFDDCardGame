using System.Collections.Generic;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class DeckCardsEvaluatableValue : CardsEvaluatableValue
    {
        public IEvaluatableValue<int> TopCardsCount { get; set; } = null;

        public override string DescribeEvaluation()
        {
            if (this.TopCardsCount == null)
            {
                return "deck";
            }

            if (this.TopCardsCount is ConstantEvaluatableValue<int> constant && constant.ConstantValue == 1)
            {
                return $"the top card of the deck";
            }

            return $"the top {this.TopCardsCount.DescribeEvaluation()} cards of the deck";
        }

        public override bool Equals(CardsEvaluatableValue other)
        {
            if (!(other is DeckCardsEvaluatableValue sameKind))
            {
                return false;
            }

            return this.TopCardsCount.Equals(sameKind.TopCardsCount);
        }

        public override string GetScriptingTokenText()
        {
            if (this.TopCardsCount == null)
            {
                return "deck";
            }

            return $"the deck {this.TopCardsCount.GetScriptingTokenText()}";
        }

        public override int RepresentingNumberOfCards(DeltaEntry toApplyTo)
        {
            TryEvaluateValue(toApplyTo.FromCampaign, toApplyTo.MadeFromBuilder, out List<Card> cards);
            int numberOfCards = cards.Count;

            if (this.TopCardsCount != null)
            {
                this.TopCardsCount.TryEvaluateValue(toApplyTo.FromCampaign, toApplyTo.MadeFromBuilder, out numberOfCards);
            }

            return UnityEngine.Mathf.Max(numberOfCards, cards.Count);
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out List<Card> evaluatedValue)
        {
            if (campaignContext?.CurrentCombatContext?.PlayerCombatDeck?.CardsCurrentlyInDeck == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Attempted to evaluate deck cards, but there isn't an appropriate combat context.", GlobalUpdateUX.LogType.RuntimeError);
                evaluatedValue = null;
                return false;
            }

            evaluatedValue = new List<Card>(campaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDeck);

            if (this.TopCardsCount != null)
            {
                if (!this.TopCardsCount.TryEvaluateValue(campaignContext, currentBuilder, out int numberOfCards))
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to evaluate number of cards to show.", GlobalUpdateUX.LogType.RuntimeError);
                    evaluatedValue = null;
                    return false;
                }

                if (evaluatedValue.Count > numberOfCards)
                {
                    evaluatedValue = evaluatedValue.GetRange(0, numberOfCards);
                }
            }

            return true;
        }
    }
}