using System.Collections.Generic;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class DiscardCardsEvaluatableValue : CardsEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "discard";
        }

        public override string GetScriptingTokenText()
        {
            return "discard";
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out List<Card> evaluatedValue)
        {
            if (campaignContext?.CurrentCombatContext?.PlayerCombatDeck?.CardsCurrentlyInDiscard == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Attempted to evaluate discard cards, but there isn't an appropriate combat context.", GlobalUpdateUX.LogType.RuntimeError);
                evaluatedValue = null;
                return false;
            }

            evaluatedValue = new List<Card>(campaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDiscard);
            return true;
        }

        public override int RepresentingNumberOfCards(DeltaEntry toApplyTo)
        {
            TryEvaluateValue(toApplyTo.FromCampaign, toApplyTo.MadeFromBuilder, out List<Card> cards);
            return cards.Count;
        }

        public override bool Equals(CardsEvaluatableValue other)
        {
            return other is DiscardCardsEvaluatableValue;
        }
    }
}