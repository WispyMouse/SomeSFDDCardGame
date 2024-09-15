namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;

    public class ElementResourceIconUX : MonoBehaviour
    {
        [SerializeReference]
        private TMPro.TMP_Text NumbericIndicator;

        [SerializeReference]
        private Image SpriteSpot;

        public Element RepresentingElement { get; private set; }

        public void SetFromElement(Element representingElement, int count)
        {
            this.RepresentingElement = representingElement;
            this.NumbericIndicator.text = count.ToString();

            if (count > 0)
            {
                this.SpriteSpot.sprite = representingElement.Sprite;
            }
            else
            {
                this.SpriteSpot.sprite = representingElement.GreyscaleSprite;
            }
        }
    }
}