namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class CardUX : MonoBehaviour
    {
        public Card RepresentedCard { get; private set; } = null;

        Action<Card> cardSelectedAction { get; set; } = null;

        public void OnMouseDown()
        {
            this.cardSelectedAction.Invoke(this.RepresentedCard);
        }

        public void SetFromCard(Card toSet, Action<Card> inCardSelectedAction)
        {
            this.RepresentedCard = toSet;
            this.cardSelectedAction = inCardSelectedAction;
        }
    }
}