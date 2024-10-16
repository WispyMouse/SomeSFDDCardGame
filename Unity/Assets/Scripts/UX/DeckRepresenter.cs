namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class DeckRepresenter : MonoBehaviour
    {
        [SerializeReference]
        private TMPro.TMP_Text CardsInDeckValue;
        [SerializeReference]
        private TMPro.TMP_Text CardsInDiscardValue;
        [SerializeReference]
        private TMPro.TMP_Text CardsInExileValue;

        private void OnEnable()
        {
            GlobalUpdateUX.UpdateUXEvent.AddListener(RepresentDeck);
        }

        private void OnDisable()
        {
            GlobalUpdateUX.UpdateUXEvent.RemoveListener(RepresentDeck);
        }

        public void RepresentDeck(CampaignContext forContext)
        {
            if (forContext == null)
            {
                this.CardsInDeckValue.text = "0";
                this.CardsInDiscardValue.text = "0";
                this.CardsInExileValue.text = "0";

                return;
            }

            if (forContext.CurrentCombatContext == null)
            {
                this.CardsInDeckValue.text = forContext.CampaignDeck.AllCardsInDeck.Count.ToString();
                this.CardsInDiscardValue.text = "0";
                this.CardsInExileValue.text = "0";
                return;
            }

            this.CardsInDeckValue.text = forContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Count.ToString();
            this.CardsInDiscardValue.text = forContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDiscard.Count.ToString();
            this.CardsInExileValue.text = forContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInExile.Count.ToString();
        }
    }
}