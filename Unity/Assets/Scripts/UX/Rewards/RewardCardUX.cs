namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class RewardCardUX : DisplayedCardUX
    {
        [SerializeField]
        private GameObject InnerPanel;

        [SerializeReference]
        public LayoutElement OwnLayoutElement;

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