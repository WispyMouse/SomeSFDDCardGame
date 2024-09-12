namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class RewardsPanelUX : MonoBehaviour
    {
        [SerializeReference]
        private RewardCardUX RewardCardUXPF;
        [SerializeReference]
        private Transform RewardCardUXHolderTransform;

        [SerializeReference]
        private CentralGameStateController CentralGameStateControllerInstance;

        private List<RewardCardUX> ActiveRewardCardUX { get; set; } = new List<RewardCardUX>();

        private void Awake()
        {
            this.DestroyRewardCards();
        }

        public void CardSelected(DisplayedCardUX selectedCard)
        {
            this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignDeck.AddCardToDeck(selectedCard.RepresentedCard);
            this.DestroyRewardCards();
            this.CentralGameStateControllerInstance.UXController.UpdateUX();
            this.gameObject.SetActive(false);
        }

        public void SetRewardCards(params Card[] toReward)
        {
            this.DestroyRewardCards();

            foreach (Card curCard in toReward)
            {
                RewardCardUX rewardedCard = Instantiate(this.RewardCardUXPF, this.RewardCardUXHolderTransform);
                rewardedCard.SetFromCard(curCard, CardSelected);
                this.ActiveRewardCardUX.Add(rewardedCard);
            }
        }

        void DestroyRewardCards()
        {
            for (int ii = RewardCardUXHolderTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(RewardCardUXHolderTransform.GetChild(ii).gameObject);
            }

            this.ActiveRewardCardUX.Clear();
        }
    }
}