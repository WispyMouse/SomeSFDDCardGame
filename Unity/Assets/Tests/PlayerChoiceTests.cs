namespace SFDDCards.Tests.EditMode
{
    using NUnit.Framework;
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ImportModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Events;

    public class PlayerChoiceTests : EditModeTestBase
    {
        [Test]
        public void ChooseAndDiscard()
        {
            CardImport import = new CardImport()
            {
                Id = nameof(ChooseAndDiscard),
                Name = nameof(ChooseAndDiscard),
                EffectScript = "[CARDTARGET: HAND][CHOOSECARDS: 1][MOVECARDTOZONE: EXILE]"
            };

            Card derivedCard = new Card(import);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(2, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            EditModeTestCommon.AddBlankCardsToPlayerDeck(combatContext.PlayerCombatDeck, 5);
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            combatContext.PlayerCombatDeck.DealCards(5);
            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(derivedCard);

            List<Card> cardsShown = null;
            UnityAction<DeltaEntry, PlayerChoice, Action> choiceHandler =
                (DeltaEntry applyTo, PlayerChoice choice, Action continuationAction) =>
            {
                PlayerChooseFromCardBrowser castChoice = choice as PlayerChooseFromCardBrowser;
                Assert.NotNull(castChoice, "The choice should be type of PlayerChooseFromCardBrowser.");

                Assert.True(castChoice.CardsToShow.TryEvaluateValue(applyTo.FromCampaign, applyTo.MadeFromBuilder, out cardsShown), "Should be able to evaluate cards to show.");

                Assert.AreEqual(cardsShown.Count, combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Count, "The cards to choose from should equal the number of cards in hand.");
                foreach (Card playerHandCard in combatContext.PlayerCombatDeck.CardsCurrentlyInHand)
                {
                    Assert.Contains(playerHandCard, cardsShown, "Expected that all cards in the player's hand are present in the displayed browser.");
                }

                castChoice.SetChoice(applyTo, new List<Card>() { cardsShown[0] });
                continuationAction.Invoke();
            };

            GlobalUpdateUX.PlayerMustMakeChoice.AddListener(choiceHandler);

            combatContext.PlayCard(derivedCard, combatContext.CombatPlayer);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.Contains(cardsShown[0], combatContext.PlayerCombatDeck.CardsCurrentlyInExile, "Expected that the chosen card has been moved to exile.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInDiscard.Contains(cardsShown[0]), "Expected that the chosen card has not been moved to discard.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Contains(cardsShown[0]), "Expected that the chosen card has not been moved to deck.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Contains(cardsShown[0]), "Expected that the chosen card has not been moved to hand.");

            Assert.Contains(derivedCard, combatContext.PlayerCombatDeck.CardsCurrentlyInDiscard, "Expected that the played card has been moved to discard, like a card that is played usually does.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInExile.Contains(derivedCard), "Expected that the chosen card has not been moved to exile.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Contains(derivedCard), "Expected that the chosen card has not been moved to deck.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Contains(derivedCard), "Expected that the chosen card has not been moved to hand.");
        }

        [Test]
        public void AutomatiallyMakeChoiceWithOneTarget()
        {
            CardImport import = new CardImport()
            {
                Id = nameof(AutomatiallyMakeChoiceWithOneTarget),
                Name = nameof(AutomatiallyMakeChoiceWithOneTarget),
                EffectScript = "[CARDTARGET: DECK][CHOOSECARDS: 1][MOVECARDTOZONE: EXILE]"
            };

            Card derivedCard = new Card(import);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(2, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(derivedCard);
            EditModeTestCommon.AddBlankCardsToPlayerDeck(combatContext.PlayerCombatDeck, 1);
            combatContext.PlayCard(derivedCard, combatContext.CombatPlayer);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.False(GlobalUpdateUX.PendingPlayerChoice, "Expecting there not to be a pending player choice.");

            Assert.AreEqual(1, combatContext.PlayerCombatDeck.CardsCurrentlyInExile.Count, "Expected one exiled card.");
        }

        [Test]
        public void AutomatiallyMakeChoiceWithNoTarget()
        {
            CardImport import = new CardImport()
            {
                Id = nameof(AutomatiallyMakeChoiceWithOneTarget),
                Name = nameof(AutomatiallyMakeChoiceWithOneTarget),
                EffectScript = "[CARDTARGET: DECK][CHOOSECARDS: 1][MOVECARDTOZONE: EXILE]"
            };

            Card derivedCard = new Card(import);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(2, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(derivedCard);
            combatContext.PlayCard(derivedCard, combatContext.CombatPlayer);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.False(GlobalUpdateUX.PendingPlayerChoice, "Expecting there not to be a pending player choice.");

            Assert.AreEqual(0, combatContext.PlayerCombatDeck.CardsCurrentlyInExile.Count, "Expected zero exiled cards.");
        }
    }
}