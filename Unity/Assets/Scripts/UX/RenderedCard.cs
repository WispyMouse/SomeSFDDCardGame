namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class RenderedCard : MonoBehaviour
    {
        [SerializeReference]
        private TMPro.TMP_Text NameText;

        [SerializeReference]
        private TMPro.TMP_Text EffectText;

        [SerializeReference]
        private Image CardImage;

        public void SetFromCard(Card representedCard)
        {
            this.NameText.text = representedCard.Name;
            this.CardImage.sprite = representedCard.Sprite;
            this.EffectText.text = representedCard.EffectText;
        }
    }
}