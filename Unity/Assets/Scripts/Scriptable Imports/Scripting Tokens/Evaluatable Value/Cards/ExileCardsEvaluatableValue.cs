using System.Collections.Generic;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class ExileCardsEvaluatableValue : CardsEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "exile";
        }

        public override string GetScriptingTokenText()
        {
            return "exile";
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out List<Card> evaluatedValue)
        {
            if (campaignContext?.CurrentCombatContext?.PlayerCombatDeck?.CardsCurrentlyInExile == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Attempted to evaluate exile cards, but there isn't an appropriate combat context.", GlobalUpdateUX.LogType.RuntimeError);
                evaluatedValue = null;
                return false;
            }

            evaluatedValue = new List<Card>(campaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInExile);
            return true;
        }

        public override int RepresentingNumberOfCards(DeltaEntry toApplyTo)
        {
            TryEvaluateValue(toApplyTo.FromCampaign, toApplyTo.MadeFromBuilder, out List<Card> cards);
            return cards.Count;
        }
    }
}