namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;


    public class RewardCardUX : DisplayedCardUX
    {
        [SerializeField]
        private GameObject InnerPanel;

        public void HideCardAfterBeingPicked()
        {
            this.InnerPanel.SetActive(false);
        }

        public override void Clicked()
        {
            this.HideCardAfterBeingPicked();
            base.Clicked();
        }
    }
}