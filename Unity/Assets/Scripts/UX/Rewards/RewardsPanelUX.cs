namespace SFDDCards.UX
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ScriptingTokens;
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

        public Reward Rewards;

        private void Awake()
        {
            this.Annihilate();
        }

        public void SetReward(Reward toReward)
        {
            this.gameObject.SetActive(true);
            this.Annihilate();

            this.Rewards = toReward;

            foreach (PickSomeReward pickReward in toReward.PickRewards)
            {
                PickXRewardPanelUX pickRewardPanel = Instantiate(this.PickRewardUXPF, this.RewardPanelsHolder);
                pickRewardPanel.RepresentPick(CentralGameStateControllerInstance.CurrentCampaignContext, this, pickReward);
            }
        }

        void Annihilate()
        {
            for (int ii = this.RewardPanelsHolder.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.RewardPanelsHolder.GetChild(ii).gameObject);
            }

            this.Rewards = null;
        }

        public void GainReward(PickSomeRewardSlot slotChosen)
        {
            this.CentralGameStateControllerInstance.CurrentCampaignContext.Gain(slotChosen);
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