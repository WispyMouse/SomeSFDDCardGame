namespace SFDDCards.UX
{
    using SFDDCards;
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

        [SerializeReference]
        private TMPro.TMP_Text QuantityLabel;

        public override bool ShouldShowBase { get; } = false;

        public void SetQuantity(int value)
        {
            if (value != 1)
            {
                this.QuantityLabel.gameObject.SetActive(true);
                this.QuantityLabel.text = $"x{value.ToString()}";
            }
            else
            {
                this.QuantityLabel.gameObject.SetActive(false);
            }
        }

        public void HideCardAfterBeingPicked()
        {
            this.InnerPanel.SetActive(false);
            this.UnHoverOnDisable();
        }

        public override void Clicked()
        {
            this.HideCardAfterBeingPicked();
            base.Clicked();
        }

        private void OnDisable()
        {
            this.UnHoverOnDisable();
        }

        private void OnDestroy()
        {
            this.UnHoverOnDisable();
        }
    }
}