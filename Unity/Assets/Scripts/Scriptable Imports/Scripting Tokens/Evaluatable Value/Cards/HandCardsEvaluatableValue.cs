using System.Collections.Generic;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class HandCardsEvaluatableValue : CardsEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "hand";
        }

        public override string GetScriptingTokenText()
        {
            return "hand";
        }

        public override bool Equals(CardsEvaluatableValue other)
        {
            return other is HandCardsEvaluatableValue;
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out List<Card> evaluatedValue)
        {
            if (campaignContext?.CurrentCombatContext?.PlayerCombatDeck?.CardsCurrentlyInHand == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Attempted to evaluate hand cards, but there isn't an appropriate combat context.", GlobalUpdateUX.LogType.RuntimeError);
                evaluatedValue = null;
                return false;
            }

            evaluatedValue = new List<Card>(campaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand);
            return true;
        }

        public override int RepresentingNumberOfCards(DeltaEntry toApplyTo)
        {
            TryEvaluateValue(toApplyTo.FromCampaign, toApplyTo.MadeFromBuilder, out List<Card> cards);
            return cards.Count;
        }
    }
}