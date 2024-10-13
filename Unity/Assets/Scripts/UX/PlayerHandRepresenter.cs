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
        [SerializeReference]
        private Transform SelectedCardTransform;

        private Dictionary<Card, CombatCardUX> CardsToRepresentations = new Dictionary<Card, CombatCardUX>();

        public ReactionWindowContext? ReactionWindowForSelectedCard
        {
            get
            {
                return this.reactionWindowForSelectedCard;
            }
            set
            {
                this.reactionWindowForSelectedCard = value;

                if (this.SelectedCard != null)
                {
                    this.SelectedCard.SetFromCard(this.SelectedCard.RepresentedCard, SelectCurrentCard, this.ReactionWindowForSelectedCard);
                }
            }
        }
        private ReactionWindowContext? reactionWindowForSelectedCard { get; set; } = null;

        public DisplayedCardUX SelectedCard { get; set; } = null;

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

            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext == null)
            {
                return;
            }

            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext == null)
            {
                this.Annihilate();
                return;
            }

            // Delete cards that are in this representer, but not in the hand
            foreach (Card key in new List<Card>(this.CardsToRepresentations.Keys))
            {
                if (this.CardsToRepresentations[key].gameObject.activeInHierarchy && !this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand.Contains(key))
                {
                    this.CardsToRepresentations[key].gameObject.SetActive(false);
                }
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
                Vector3 downpush = Vector3.down * Mathf.InverseLerp(0, Mathf.Sin(Mathf.Deg2Rad * FanDegreesMaximum), Mathf.Sin(Mathf.Deg2Rad * Mathf.Abs(thisCardAngle))) * MaximumDownwardsness;

                // And where it should be positioned
                Vector3 objectOffset = new Vector3(leftStartingPoint, 0, 0) + new Vector3(modifiedCardFanDistance, 0, 0) * ii + backpush + downpush;

                CombatCardUX newCard = GetUX(this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand[ii]);

                if (ReferenceEquals(this.SelectedCard, newCard))
                {
                    objectOffset += Vector3.up * 1f;
                    newCard.transform.parent = this.SelectedCardTransform;
                }
                else
                {
                    newCard.transform.parent = this.PlayerHandTransform;
                }

                newCard.SetTargetPosition(newCard.transform.parent.position + objectOffset, -thisCardAngle);

                // Does the player meet the requirements of at least one of the effects?
                bool anyPassingRequirements = ScriptTokenEvaluator.MeetsAnyRequirements(
                    ScriptTokenEvaluator.CalculateConceptualBuildersFromTokenEvaluation(newCard.RepresentedCard),
                    this.CentralGameStateControllerInstance.CurrentCampaignContext, 
                    this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer,
                    this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer,
                    null);
                newCard.RequirementsAreMet = anyPassingRequirements;
            }
        }

        public void Annihilate()
        {
            foreach (CombatCardUX ux in new List<CombatCardUX>(this.CardsToRepresentations.Values))
            {
                Destroy(ux.gameObject);
            }

            this.CardsToRepresentations.Clear();
        }

        public void SelectCurrentCard(DisplayedCardUX selectedCard)
        {
            this.SelectedCard = selectedCard;
            this.SelectedCard.SetFromCard(this.SelectedCard.RepresentedCard, SelectCurrentCard, GetReactionWindowContextForCard(selectedCard));
            this.UXController.SelectCurrentCard(selectedCard);
            this.RepresentPlayerHand();
        }

        public void DeselectSelectedCard()
        {
            if (this.SelectedCard != null)
            {
                this.SelectedCard.SetFromCard(this.SelectedCard.RepresentedCard, SelectCurrentCard, null);
                this.SelectedCard = null;
                this.RepresentPlayerHand();
            }
        }

        public CombatCardUX GetUX(Card forCard)
        {
            bool wasNotVisibleOrJustCreated = false;

            if (!this.CardsToRepresentations.TryGetValue(forCard, out CombatCardUX representingCard))
            {
                representingCard = Instantiate(this.CardRepresentationPF, this.PlayerHandTransform);
                representingCard.SetFromCard(forCard, SelectCurrentCard, null);
                this.CardsToRepresentations.Add(forCard, representingCard);
                wasNotVisibleOrJustCreated = true;
            }
            else if (!this.CardsToRepresentations[forCard].isActiveAndEnabled)
            {
                representingCard.gameObject.SetActive(true);
                representingCard.SetFromCard(forCard, SelectCurrentCard, null);
                wasNotVisibleOrJustCreated = true;
            }

            if (wasNotVisibleOrJustCreated)
            {
                representingCard.SnapToPosition(this.PlayerHandTransform.position);
            }

            if (ReferenceEquals(this.SelectedCard, representingCard))
            {
                this.SelectedCard.SetFromCard(forCard, SelectCurrentCard, this.ReactionWindowForSelectedCard);
            }

            return representingCard;
        }

        public ReactionWindowContext? GetReactionWindowContextForCard(DisplayedCardUX ux)
        {
            if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext == null)
            {
                return null;
            }

            return new ReactionWindowContext()
            {
                CampaignContext = this.CentralGameStateControllerInstance.CurrentCampaignContext,
                CombatantEffectOwner = this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer,
                CombatantTarget = UXController.HoveredCombatant,
                PlayedFromZone = "hand",
                TimingWindowId = KnownReactionWindows.ConsideringPlayingFromHand
            };
        }
    }
}