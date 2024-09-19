namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class MouseHoverShowerPanel : MonoBehaviour
    {
        [SerializeReference]
        private RenderedCard OverlayCard;

        [SerializeReference]
        public RectTransform OwnTransform;

        public void SetFromHoverListener(IMouseHoverListener listener)
        {
            if (listener.TryGetCard(out Card toShow))
            {
                this.ShowCard(toShow);
            }
            else
            {
                this.HideCard();
            }

            foreach (Graphic curGraphic in this.GetComponentsInChildren<Graphic>(true))
            {
                curGraphic.raycastTarget = false;
            }
        }

        private void ShowCard(Card toShow)
        {
            this.OverlayCard.gameObject.SetActive(true);
            this.OverlayCard.SetFromCard(toShow);
        }

        private void HideCard()
        {
            this.OverlayCard.Annihilate();
            this.OverlayCard.gameObject.SetActive(false);
        }
    }
}