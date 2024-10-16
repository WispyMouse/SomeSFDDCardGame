namespace SFDDCards.UX
{
    using SFDDCards.ImportModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class RewardCurrencyUX : MonoBehaviour, IMouseHoverListener
    {
        [SerializeReference]
        private Image ImageRepresentation;

        [SerializeReference]
        private TMPro.TMP_Text Name;

        [SerializeReference]
        private TMPro.TMP_Text Label;

        [SerializeReference]
        public LayoutElement OwnLayoutElement;

        public CurrencyImport RepresentedCurrency;
        public int RewardAmount;

        private Action<RewardCurrencyUX> OnClicked { get; set; }

        public bool ShouldShowBase => true;

        public void SetFromCurrency(CurrencyImport basedOn, Action<RewardCurrencyUX> onClick, int amount)
        {
            this.RepresentedCurrency = basedOn;

            this.ImageRepresentation.sprite = basedOn.CurrencyArt;
            this.Name.text = basedOn.Name;
            this.RewardAmount = amount;
            this.Label.text = $"x{RewardAmount.ToString()} {basedOn.GetNameAndMaybeIcon()}";
            this.OnClicked = onClick;
        }

        public void Clicked()
        {
            this.gameObject.SetActive(false);
            this.OnClicked.Invoke(this);
        }

        public virtual void MouseEnterStartHover()
        {
            MouseHoverShowerController.MouseStartHoveredEvent.Invoke(this);
        }

        public virtual void MouseExitStopHover()
        {
            MouseHoverShowerController.MouseEndHoveredEvent.Invoke(this);
        }

        public Transform GetTransform()
        {
            return this.transform;
        }

        public bool TryGetCard(out Card toShow)
        {
            toShow = null;
            return false;
        }

        public bool TryGetStatusEffect(out IStatusEffect toShow)
        {
            toShow = null;
            return false;
        }

        public void UnHoverOnDisable()
        {
            MouseHoverShowerController.MouseEndHoveredEvent.Invoke(this);
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