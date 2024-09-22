namespace SFDDCards.UX
{
    using SFDDCards.Evaluation.Actual;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class RewardsPanelUX : MonoBehaviour
    {
        [SerializeReference]
        private PickXRewardPanelUX PickRewardUXPF;
        [SerializeReference]
        private Transform RewardPanelsHolder;

        [SerializeReference]
        private CentralGameStateController CentralGameStateControllerInstance;

        private void Awake()
        {
            this.Annihilate();
        }

        public void SetReward(Reward toReward)
        {
            this.Annihilate();

            foreach (PickSomeReward pickReward in toReward.PickRewards)
            {
                PickXRewardPanelUX pickRewardPanel = Instantiate(this.PickRewardUXPF, this.RewardPanelsHolder);
                pickRewardPanel.RepresentPick(this, pickReward);
            }

            this.gameObject.SetActive(true);
        }

        void Annihilate()
        {
            for (int ii = this.RewardPanelsHolder.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.RewardPanelsHolder.GetChild(ii).gameObject);
            }
        }

        public void GainReward(PickSomeRewardSlot slotChosen)
        {
            if (slotChosen.RewardedCard != null)
            {
                this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignDeck.AddCardToDeck(slotChosen.RewardedCard);
            }
            else if (slotChosen.RewardedEffect != null)
            {
                this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer.ApplyDelta(
                    this.CentralGameStateControllerInstance.CurrentCampaignContext,
                    null,
                    new DeltaEntry()
                    {
                        Target = this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer,
                        User = this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer,
                        StatusEffect = slotChosen.RewardedEffect,
                        Intensity = 1,
                        IntensityKindType = TokenEvaluatorBuilder.IntensityKind.ApplyStatusEffect
                    });
            }

            GlobalUpdateUX.UpdateUXEvent?.Invoke();
        }

        public void ClosePanel(PickXRewardPanelUX toClose)
        {
            Destroy(toClose.gameObject);
        }

        public void CloseAll()
        {
            this.gameObject.SetActive(false);
        }
    }
}