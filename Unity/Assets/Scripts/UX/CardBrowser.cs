namespace SFDDCards.UX
{
    using SFDDCards.Evaluation.Actual;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using UnityEngine;


    public class CardBrowser : MonoBehaviour
    {
        [SerializeReference]
        private Transform CardHolder;

        [SerializeReference]
        private RenderedCard RenderedCardPF;

        [SerializeReference]
        private TMPro.TMP_Text Label;

        [SerializeField]
        private GameObject CloseButton;

        private Action<List<Card>> SelectionFinishedAction { get; set; } = null;
        public int RemainingCardsToChoose { get; set; } = 0;
        private List<Card> ChosenCards { get; set; } = new List<Card>();

        public void Awake()
        {
            this.Annihilate();
        }

        public void SetLabelText(string newText)
        {
            this.Label.text = newText;
        }

        public void SetFromCards(IEnumerable<Card> cardsToShow)
        {
            this.gameObject.SetActive(true);
            this.Annihilate();
            this.CloseButton.SetActive(true);

            foreach (Card curCard in cardsToShow)
            {
                RenderedCard newCard = Instantiate(this.RenderedCardPF, this.CardHolder);
                newCard.SetFromCard(curCard);
                newCard.OnClickAction = this.CardClicked;
            }
        }

        public void Close()
        {
            this.Annihilate(false);
            this.gameObject.SetActive(false);
            GlobalUpdateUX.UpdateUXEvent.Invoke();
        }

        public void Annihilate(bool close = true)
        {
            for (int ii = this.CardHolder.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.CardHolder.GetChild(ii).gameObject);
            }

            this.ChosenCards = new List<Card>();
            this.RemainingCardsToChoose = 0;

            if (close)
            {
                this.Close();
            }
        }

        public void SetFromCardBrowserChoice(DeltaEntry fromDelta, PlayerChooseFromCardBrowser toHandle, Action<List<Card>> continuationAction)
        {
            if (!toHandle.NumberOfCardsToChoose.TryEvaluateValue(fromDelta.FromCampaign, fromDelta.MadeFromBuilder, out int evaluatedNumberOfCards))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to evaluate number of cards for browser choice.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            if (!toHandle.CardsToShow.TryEvaluateValue(fromDelta.FromCampaign, fromDelta.MadeFromBuilder, out List<Card> evaluatedCards))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to evaluate card zone for browser choice.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            evaluatedNumberOfCards = Mathf.Min(evaluatedNumberOfCards, evaluatedCards.Count);

            if (evaluatedNumberOfCards == 0)
            {
                this.Close();
                continuationAction.Invoke(new List<Card>());
                return;
            }

            this.SetFromCards(evaluatedCards);
            this.SetLabelText(toHandle.DescribeChoice(fromDelta.FromCampaign, fromDelta.MadeFromBuilder));
            this.RemainingCardsToChoose = evaluatedNumberOfCards;
            this.CloseButton.SetActive(false);
            SelectionFinishedAction = continuationAction;
        }

        public void CardClicked(RenderedCard chosenCard)
        {
            if (this.RemainingCardsToChoose == 0)
            {
                return;
            }

            this.ChosenCards.Add(chosenCard.RepresentedCard);
            Destroy(chosenCard.gameObject);

            this.RemainingCardsToChoose--;

            if (this.RemainingCardsToChoose == 0)
            {
                List<Card> chosenCards = new List<Card>(this.ChosenCards);
                this.Close();
                this.SelectionFinishedAction?.Invoke(chosenCards);
            }
        }
    }
}
