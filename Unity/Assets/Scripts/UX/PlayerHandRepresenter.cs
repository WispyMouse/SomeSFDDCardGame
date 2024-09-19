namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class PlayerHandRepresenter : MonoBehaviour
    {
        [SerializeReference]
        private GameplayUXController UXController;
        [SerializeReference]
        private CentralGameStateController CentralGameStateControllerInstance;

        [SerializeReference]
        private CombatCardUX CardRepresentationPF;

        [SerializeReference]
        private Transform PlayerHandTransform;

        private void OnEnable()
        {
            GlobalUpdateUX.UpdateUXEvent.AddListener(RepresentPlayerHand);
        }

        private void OnDisable()
        {
            GlobalUpdateUX.UpdateUXEvent.RemoveListener(RepresentPlayerHand);
        }

        public void RepresentPlayerHand()
        {
            const float MaximumCardFanDistance = 8f;
            const float CardFanDistance = 1.4f;
            const float FanDegreesMaximum = 30f;
            const int CountForMaximumFanValue = 20;

            const float MaximumDownwardsness = .6f;

            for (int ii = this.PlayerHandTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.PlayerHandTransform.GetChild(ii).gameObject);
            }

            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext == null)
            {
                return;
            }

            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext == null)
            {
                this.Annihilate();
                return;
            }

            // Identify the angle to fan things out
            // Cards in the center are less rotated than cards on the ends
            int cardsInHand = this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand.Count;

            if (cardsInHand == 0)
            {
                return;
            }

            float cardsMinusOne = (float)(cardsInHand - 1);

            float modifiedCardFanDistance = cardsInHand > 1 ? Mathf.Min(CardFanDistance * (float)cardsInHand, MaximumCardFanDistance) / cardsMinusOne : 0;
            float leftStartingPoint = -modifiedCardFanDistance * (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand.Count - 1) / 2f;
            float maxFanAngle = cardsInHand > CountForMaximumFanValue ? FanDegreesMaximum : Mathf.Lerp(0, FanDegreesMaximum, cardsMinusOne / (float)CountForMaximumFanValue);
            float fanAnglePerIndex = cardsInHand > 1 ? maxFanAngle / cardsMinusOne * 2f : 0;
            float leftStartingPointAngle = -maxFanAngle;

            for (int ii = 0; ii < cardsInHand; ii++)
            {
                // Push the cards in the right side of the hand slightly back, so that the edge with the elments on the right overlays properly
                // forward is away from the camera, so it's "further back"
                Vector3 backpush = ii * Vector3.forward * .005f;

                // Identify the angle that this should be at
                float thisCardAngle = leftStartingPointAngle + ii * fanAnglePerIndex;

                // Push the cards that are rotated down so they look more like a fan
                Vector3 downpush = Vector3.down * Mathf.InverseLerp(0, Mathf.Sin(FanDegreesMaximum), Mathf.Sin(Mathf.Abs(thisCardAngle))) * MaximumDownwardsness;

                // And where it should be positioned
                Vector3 objectOffset = new Vector3(leftStartingPoint, 0, 0) + new Vector3(modifiedCardFanDistance, 0, 0) * ii + backpush + downpush;
                
                CombatCardUX newCard = Instantiate(this.CardRepresentationPF, this.PlayerHandTransform);
                newCard.transform.localPosition = objectOffset;
                newCard.transform.localRotation = Quaternion.Euler(0, 0, -thisCardAngle);
                newCard.SetFromCard(this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand[ii], SelectCurrentCard);
            }
        }

        public void Annihilate()
        {
            for (int ii = this.PlayerHandTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.PlayerHandTransform.GetChild(ii).gameObject);
            }
        }

        public void SelectCurrentCard(DisplayedCardUX selectedCard)
        {
            this.UXController.SelectCurrentCard(selectedCard);
        }
    }
}