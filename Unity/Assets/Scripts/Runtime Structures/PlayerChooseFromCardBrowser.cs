namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public class PlayerChooseFromCardBrowser : PlayerChoice<List<Card>>
    {
        public PromisedCardsEvaluatableValue CardsToShow;
        public IEvaluatableValue<int> NumberOfCardsToChoose;

        public PlayerChooseFromCardBrowser(PromisedCardsEvaluatableValue cardsToShow, IEvaluatableValue<int> numberOfCards)
        {
            this.CardsToShow = cardsToShow;
            this.NumberOfCardsToChoose = numberOfCards;
        }

        public override string DescribeChoice(CampaignContext campaignContext, TokenEvaluatorBuilder currentEvaluator)
        {
            int numberOfCards = 0;
            if (!this.NumberOfCardsToChoose.TryEvaluateValue(campaignContext, currentEvaluator, out numberOfCards))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse number of cards for choice.", GlobalUpdateUX.LogType.RuntimeError);
            }

            return $"Choose {numberOfCards} cards from {CardsToShow.DescribeEvaluation()}";
        }

        public override void SetChoice(DeltaEntry toApplyTo, List<Card> result)
        {
            base.SetChoice(toApplyTo, result);
            this.CardsToShow.InnerValue = new SpecificCardsEvaluatableValue(result);
        }

        public override bool TryFinalizeWithoutPlayerInput(DeltaEntry toApplyTo)
        {
            if (!this.NumberOfCardsToChoose.TryEvaluateValue(toApplyTo.FromCampaign, toApplyTo.MadeFromBuilder, out int numberOfCards))
            {
                return false;
            }

            if (this.CardsToShow.InnerValue == null && this.CardsToShow.SampledPool != null && numberOfCards >= this.CardsToShow.SampledPool.RepresentingNumberOfCards(toApplyTo) && this.CardsToShow.SampledPool.TryEvaluateValue(toApplyTo.FromCampaign, toApplyTo.MadeFromBuilder, out List<Card> chosenCards))
            {
                this.CardsToShow.InnerValue = new SpecificCardsEvaluatableValue(chosenCards);
                this.SetChoice(toApplyTo, chosenCards);
                return true;
            }

            if (this.CardsToShow.InnerValue != null && numberOfCards >= this.CardsToShow.InnerValue.RepresentingNumberOfCards(toApplyTo) && this.CardsToShow.InnerValue.TryEvaluateValue(toApplyTo.FromCampaign, toApplyTo.MadeFromBuilder, out chosenCards))
            {
                this.CardsToShow.InnerValue = new SpecificCardsEvaluatableValue(chosenCards);
                this.SetChoice(toApplyTo, chosenCards);
                return true;
            }

            return false;
        }
    }
}