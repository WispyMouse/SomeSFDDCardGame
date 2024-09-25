namespace SFDDCards.UX
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class RarityIndicator : MonoBehaviour
    {
        [SerializeReference]
        private Image IconShower;

        [SerializeReference]
        private Sprite StarterRarityIcon;

        [SerializeReference]
        private Sprite CommonRarityIcon;

        [SerializeReference]
        private Sprite UncommonRarityIcon;

        [SerializeReference]
        private Sprite RareRarityIcon;


        public void SetFromRarity(Card.KnownRarities rarity)
        {
            switch (rarity)
            {
                case Card.KnownRarities.Unknown:
                    this.IconShower.gameObject.SetActive(false);
                    break;
                case Card.KnownRarities.Starter:
                    this.IconShower.gameObject.SetActive(true);
                    this.IconShower.sprite = this.StarterRarityIcon;
                    break;
                case Card.KnownRarities.Common:
                    this.IconShower.gameObject.SetActive(true);
                    this.IconShower.sprite = this.CommonRarityIcon;
                    break;
                case Card.KnownRarities.Uncommon:
                    this.IconShower.gameObject.SetActive(true);
                    this.IconShower.sprite = this.UncommonRarityIcon;
                    break;
                case Card.KnownRarities.Rare:
                    this.IconShower.gameObject.SetActive(true);
                    this.IconShower.sprite = this.RareRarityIcon;
                    break;
            }
        }
    }
}