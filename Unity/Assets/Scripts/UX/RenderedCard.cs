namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class RenderedCard : MonoBehaviour
    {
        public Card RepresentedCard;

        [SerializeReference]
        private TMPro.TMP_Text NameText;

        [SerializeReference]
        private TMPro.TMP_Text EffectText;

        [SerializeReference]
        private Image CardImage;

        [SerializeReference]
        private ElementResourceIconUX ElementResourceIconUXPF;

        [SerializeReference]
        private Transform ElementResourceIconHolder;

        [SerializeReference]
        private RarityIndicator MyRarityIndicator;

        public Action<RenderedCard> OnClickAction { get; set; } = null;

        public void SetFromCard(Card representedCard, ReactionWindowContext? reactionWindowContext = null)
        {
            this.Annihilate();

            this.RepresentedCard = representedCard;

            this.NameText.text = representedCard.Name;
            this.CardImage.sprite = representedCard.Sprite;
            this.EffectText.text = representedCard.GetDescription(reactionWindowContext).BreakDescriptionsIntoString();

            foreach (Element curElement in ElementDatabase.ElementData.Values)
            {
                ElementResourceIconUX icon = Instantiate(this.ElementResourceIconUXPF, this.ElementResourceIconHolder);
                icon.SetFromElement(curElement, representedCard.GetElementGain(curElement));
            }

            this.MyRarityIndicator.SetFromRarity(representedCard.Rarity);
        }

        public void Annihilate()
        {
            this.NameText.text = "";
            this.CardImage.sprite = null;
            this.EffectText.text = "";

            for (int ii = this.ElementResourceIconHolder.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.ElementResourceIconHolder.GetChild(ii).gameObject);
            }
        }

        public void Clicked()
        {
            this.OnClickAction?.Invoke(this);
        }
    }
}