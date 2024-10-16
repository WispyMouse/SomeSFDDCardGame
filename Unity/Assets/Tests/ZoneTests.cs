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

    public class ZoneTests : EditModeTestBase
    {
        [Test]
        public void PlayedCardIsDiscarded()
        {
            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(2, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            EditModeTestCommon.AddBlankCardsToPlayerDeck(combatContext.PlayerCombatDeck, 5);
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            combatContext.PlayerCombatDeck.DealCards(5);
            Card firstCard = combatContext.PlayerCombatDeck.CardsCurrentlyInHand[0];

            combatContext.PlayCard(firstCard, combatContext.CombatPlayer);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            Assert.Contains(firstCard, combatContext.PlayerCombatDeck.CardsCurrentlyInDiscard, "Expected that the chosen card has been moved to discard.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInExile.Contains(firstCard), "Expected that the chosen card has not been moved to exile.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Contains(firstCard), "Expected that the chosen card has not been moved to deck.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Contains(firstCard), "Expected that the chosen card has not been moved to hand.");
        }

        [Test]
        public void PlayedCardIsExiledIntentionally()
        {
            CardImport import = new CardImport()
            {
                Id = nameof(PlayedCardIsExiledIntentionally),
                Name = nameof(PlayedCardIsExiledIntentionally),
                EffectScript = "[CARDTARGET: SELF][MOVECARDTOZONE: EXILE]"
            };

            Card derivedCard = new Card(import);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(2, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            EditModeTestCommon.AddBlankCardsToPlayerDeck(combatContext.PlayerCombatDeck, 5);
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            combatContext.PlayerCombatDeck.DealCards(5);
            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(derivedCard);
            combatContext.PlayCard(derivedCard, combatContext.CombatPlayer);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            Assert.Contains(derivedCard, combatContext.PlayerCombatDeck.CardsCurrentlyInExile, "Expected that the chosen card has been moved to exile.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInDiscard.Contains(derivedCard), "Expected that the chosen card has not been moved to discard.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Contains(derivedCard), "Expected that the chosen card has not been moved to deck.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Contains(derivedCard), "Expected that the chosen card has not been moved to hand.");
        }

        [Test]
        public void PlayedCardIsReturnedToHand()
        {
            CardImport import = new CardImport()
            {
                Id = nameof(PlayedCardIsReturnedToHand),
                Name = nameof(PlayedCardIsReturnedToHand),
                EffectScript = "[CARDTARGET: SELF][MOVECARDTOZONE: HAND]"
            };

            Card derivedCard = new Card(import);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(2, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            EditModeTestCommon.AddBlankCardsToPlayerDeck(combatContext.PlayerCombatDeck, 5);
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            combatContext.PlayerCombatDeck.DealCards(5);
            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(derivedCard);
            combatContext.PlayCard(derivedCard, combatContext.CombatPlayer);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            Assert.Contains(derivedCard, combatContext.PlayerCombatDeck.CardsCurrentlyInHand, "Expected that the chosen card has been moved to dihandscard.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInExile.Contains(derivedCard), "Expected that the chosen card has not been moved to exile.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Contains(derivedCard), "Expected that the chosen card has not been moved to deck.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInDiscard.Contains(derivedCard), "Expected that the chosen card has not been moved to discard.");
        }

        [Test]
        public void ExiledCardsDoNotShuffleIn()
        {
            CardImport import = new CardImport()
            {
                Id = nameof(PlayedCardIsReturnedToHand),
                Name = nameof(PlayedCardIsReturnedToHand),
                EffectScript = ""
            };

            Card derivedCard = new Card(import);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(2, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            EditModeTestCommon.AddBlankCardsToPlayerDeck(combatContext.PlayerCombatDeck, 5);
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            combatContext.PlayerCombatDeck.DealCards(5);
            combatContext.PlayerCombatDeck.CardsCurrentlyInExile.Add(derivedCard);
            combatContext.PlayerCombatDeck.ShuffleDiscardIntoDeck();

            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            Assert.Contains(derivedCard, combatContext.PlayerCombatDeck.CardsCurrentlyInExile, "Expected that the card is still in exile.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Contains(derivedCard), "Expected that the chosen card has not been moved to hand.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Contains(derivedCard), "Expected that the chosen card has not been moved to deck.");
            Assert.False(combatContext.PlayerCombatDeck.CardsCurrentlyInDiscard.Contains(derivedCard), "Expected that the chosen card has not been moved to discard.");
        }

        [Test]
        public void CardAddedToCampaignDeck()
        {
            CardImport cardToGenerate = new CardImport()
            {
                Id = nameof(cardToGenerate),
                Name = nameof(cardToGenerate),
                EffectScript = ""
            };

            CardDatabase.AddCardToDatabase(cardToGenerate);

            CardImport generatingCard = new CardImport()
            {
                Id = nameof(generatingCard),
                Name = nameof(generatingCard),
                EffectScript = $"[GENERATECARD: {cardToGenerate.Id} 5][COPYANDADDTOCAMPAIGNDECK]"
            };

            Card derivedCard = new Card(generatingCard);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(2, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(derivedCard);
            combatContext.PlayCard(derivedCard, combatContext.CombatPlayer);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            Assert.IsTrue(campaignContext.CampaignDeck.AllCardsInDeck.Count == 5, "Expected that there should be five separate copies of the generated card in the campaign deck.");

            List<Card> deck = new List<Card>(campaignContext.CampaignDeck.AllCardsInDeck);
            for (int ii = 0; ii < 5; ii++)
            {
                Card thisCard = deck[ii];
                Assert.AreEqual(nameof(cardToGenerate).ToLower(), thisCard.Id, "Expecting each generated card to be of the appropriate type.");

                for (int jj = ii + 1; jj < 5; jj++)
                {
                    Card compareTo = deck[jj];

                    Assert.AreNotEqual(thisCard, compareTo, "Expecting each card instance to be separate.");
                }
            }

            Assert.AreEqual(0, combatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Count, "Expecting generated cards to not have been added to the deck.");
        }
    }
}