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
        private CentralGameStateController CentralGameStateControllerInstance;

        [SerializeReference]
        private GameObject DeckStatPanel;
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

        public void RepresentDeck()
        {
            if (this.CentralGameStateControllerInstance.CurrentCampaignContext == null)
            {
                this.CardsInDeckValue.text = "0";
                this.CardsInDiscardValue.text = "0";
                this.CardsInExileValue.text = "0";

                return;
            }

            if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext == null)
            {
                this.CardsInDeckValue.text = this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignDeck.AllCardsInDeck.Count.ToString();
                this.CardsInDiscardValue.text = "0";
                this.CardsInExileValue.text = "0";
                return;
            }

            this.CardsInDeckValue.text = this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Count.ToString();
            this.CardsInDiscardValue.text = this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDiscard.Count.ToString();
            this.CardsInExileValue.text = this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInExile.Count.ToString();
        }
    }
}