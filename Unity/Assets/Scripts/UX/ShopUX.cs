namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class ShopUX : MonoBehaviour
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
            Destroy(selectedCard.gameObject);
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

            GlobalUpdateUX.UpdateUXEvent?.Invoke();
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