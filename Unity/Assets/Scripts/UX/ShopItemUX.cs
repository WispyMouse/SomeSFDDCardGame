namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class ShopItemUX : MonoBehaviour
    {
        public ShopEntry RepresentingEntry { get; set; }

        [SerializeReference]
        private RewardCardUX RewardCardPF;

        [SerializeReference]
        private RewardArtifactUX RewardArtifactPF;

        [SerializeReference]
        private RewardCurrencyUX RewardCurrencyPF;

        [SerializeReference]
        private Transform RewardCardHolder;

        [SerializeReference]
        private TMPro.TMP_Text CostsLabel;

        private Action<ShopItemUX> OnClickDelegate;

        [SerializeReference]
        private GameObject CanNotAffordOverlay;

        public void SetFromEntry(CampaignContext forCampaign, ShopEntry toRepresent, Action<ShopItemUX> onClickDelegate)
        {
            if (!toRepresent.GainedAmount.TryEvaluateValue(forCampaign, null, out int gainedAmount))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Could not evaluate gain amount.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            this.RepresentingEntry = toRepresent;
            this.OnClickDelegate = onClickDelegate;
            this.RepresentCosts(toRepresent.Costs, forCampaign);

            if (toRepresent.GainedCard != null)
            {
                RewardCardUX thisCard = Instantiate(this.RewardCardPF, this.RewardCardHolder);
                thisCard.SetFromCard(toRepresent.GainedCard, (DisplayedCardUX card) => { this.OnClick(); });
                thisCard.SetQuantity(gainedAmount);
            }
            else if (toRepresent.GainedEffect != null)
            {
                RewardArtifactUX rewardArtifact = Instantiate(this.RewardArtifactPF, this.RewardCardHolder);
                rewardArtifact.SetFromArtifact(toRepresent.GainedEffect, (RewardArtifactUX artifact) => { this.OnClick(); }, gainedAmount);
            }
            else if (toRepresent.GainedCurrency != null)
            {
                RewardCurrencyUX rewardCurrency = Instantiate(this.RewardCurrencyPF, this.RewardCardHolder);
                rewardCurrency.SetFromCurrency(toRepresent.GainedCurrency, (RewardCurrencyUX currency) => { this.OnClick(); }, gainedAmount);
            }
        }

        public void OnClick()
        {
            this.OnClickDelegate.Invoke(this);
        }

        void RepresentCosts(List<ShopCost> costs, CampaignContext forCampaign)
        {
            if (costs.Count == 0)
            {
                this.CostsLabel.text = "Free!";
                return;
            }

            string startingSeparator = "";
            StringBuilder compositeCurrencies = new StringBuilder();
            foreach (ShopCost cost in costs)
            {
                if (!cost.Amount.TryEvaluateValue(forCampaign, null, out int costAmount))
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Could not parse shop cost.", GlobalUpdateUX.LogType.RuntimeError);
                    continue;
                }

                compositeCurrencies.Append($"{startingSeparator}{costAmount.ToString()}\u00A0{cost.Currency.GetNameAndMaybeIcon()}");
                startingSeparator = ", ";
            }
            this.CostsLabel.text = compositeCurrencies.ToString();
        }

        private void OnEnable()
        {
            GlobalUpdateUX.UpdateUXEvent.AddListener(this.UpdateAffordability);
        }

        private void OnDisable()
        {
            GlobalUpdateUX.UpdateUXEvent.RemoveListener(this.UpdateAffordability);
        }

        public void UpdateAffordability(CampaignContext campaignContext)
        {
            if (!campaignContext.CanAfford(this.RepresentingEntry.Costs))
            {
                this.CanNotAffordOverlay.gameObject.SetActive(true);
                return;
            }

            this.CanNotAffordOverlay.gameObject.SetActive(false);
        }
    }
}