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

        Action<CardUX> cardSelectedAction { get; set; } = null;

        [SerializeReference]
        private TMPro.TMP_Text NameText;

        [SerializeReference]
        private TMPro.TMP_Text EffectText;

        [SerializeReference]
        private GameObject SelectedGlow;

        public void Awake()
        {
            this.DisableSelectionGlow();
        }

        public void Clicked()
        {
            this.cardSelectedAction.Invoke(this);
        }

        public void SetFromCard(Card toSet, Action<CardUX> inCardSelectedAction)
        {
            this.RepresentedCard = toSet;
            this.cardSelectedAction = inCardSelectedAction;

            this.NameText.text = toSet.Name;
            this.EffectText.text = toSet.EffectText;
        }

        public void EnableSelectionGlow()
        {
            this.SelectedGlow.SetActive(true);
        }

        public void DisableSelectionGlow()
        {
            this.SelectedGlow.SetActive(false);
        }
    }
}