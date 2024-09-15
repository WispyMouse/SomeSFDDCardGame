namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class DisplayedCardUX : MonoBehaviour, IMouseHoverListener
    {
        public Card RepresentedCard { get; private set; } = null;
        
        [SerializeReference]
        private RenderedCard RenderedCard;

        Action<DisplayedCardUX> cardSelectedAction { get; set; } = null;

        public void Awake()
        {
            this.DisableSelectionGlow();
        }

        public virtual void MouseEnterStartHover()
        {
            GlobalUpdateUX.MouseStartHoveredEvent.Invoke(this);
        }

        public virtual void MouseExitStopHover()
        {
            GlobalUpdateUX.MouseEndHoveredEvent.Invoke(this);
        }

        public virtual void Clicked()
        {
            this.cardSelectedAction.Invoke(this);
        }

        public virtual void EnableSelectionGlow()
        {
        }

        public virtual void DisableSelectionGlow()
        {
        }

        public void SetFromCard(Card toSet, Action<DisplayedCardUX> inCardSelectedAction)
        {
            this.RepresentedCard = toSet;
            this.cardSelectedAction = inCardSelectedAction;

            this.RenderedCard.SetFromCard(toSet);
        }

        public Transform GetTransform()
        {
            return this.transform;
        }

        public bool TryGetCard(out Card toShow)
        {
            toShow = this.RenderedCard?.RepresentedCard;
            return toShow != null;
        }
    }
}