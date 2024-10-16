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
    using System.Threading.Tasks;
    using UnityEngine;


    public class CompositeEvaluatableTests : EditModeTestBase
    {
        [Test]
        public void Division_RoundsDown()
        {
            CardImport import = new CardImport()
            {
                Id = nameof(Division_RoundsDown),
                Name = nameof(Division_RoundsDown),
                EffectScript = $"[SETTARGET: FOE][DAMAGE: 10/3]"
            };

            CardDatabase.AddCardToDatabase(import);
            Card testGainCard = CardDatabase.GetModel(import.Id);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(testGainCard);
            combatContext.PlayCard(testGainCard, combatContext.Enemies[0]);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            Assert.AreEqual(97, combatContext.Enemies[0].CurrentHealth, "The effect should have dealt exactly three damage.");
        }

        [Test]
        public void Division_RoundsUp()
        {
            CardImport import = new CardImport()
            {
                Id = nameof(Division_RoundsDown),
                Name = nameof(Division_RoundsDown),
                EffectScript = $"[SETTARGET: FOE][DAMAGE: 8/3]"
            };

            CardDatabase.AddCardToDatabase(import);
            Card testGainCard = CardDatabase.GetModel(import.Id);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(testGainCard);
            combatContext.PlayCard(testGainCard, combatContext.Enemies[0]);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            Assert.AreEqual(97, combatContext.Enemies[0].CurrentHealth, "The effect should have dealt exactly three damage.");
        }

        /// <summary>
        /// Attempts to hit all numbers from 0-10.
        /// This should happen within a reasonable timeframe, right?
        /// </summary>
        [Test]
        public void Randomization_Roundtrip()
        {
            CardImport import = new CardImport()
            {
                Id = nameof(Division_RoundsDown),
                Name = nameof(Division_RoundsDown),
                EffectScript = $"[SETTARGET: FOE][DAMAGE: 0~10]"
            };

            CardDatabase.AddCardToDatabase(import);
            Card testGainCard = CardDatabase.GetModel(import.Id);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 1000000000);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            HashSet<int> numbersSeen = new HashSet<int>();
            int lastHealth = combatContext.Enemies[0].CurrentHealth;

            for (int ii = 0; ii < 1000; ii++)
            {
                combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(testGainCard);
                combatContext.PlayCard(testGainCard, combatContext.Enemies[0]);
                GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

                int newHealth = combatContext.Enemies[0].CurrentHealth;
                int damageDone = lastHealth - newHealth;
                lastHealth = newHealth;

                if (damageDone < 0 || damageDone > 10)
                {
                    Assert.Fail($"Rolled {damageDone}, but all numbers should be from 0 to 10 inclusive.");
                }

                numbersSeen.Add(damageDone);

                if (numbersSeen.Count == 11)
                {
                    break;
                }
            }

            if (numbersSeen.Count != 11)
            {
                Assert.Fail($"Should have seen all numbers from 0-10 by the end of the sequence. (11 numbers) It is fathomable that this is a stochastic error and would resolve upon running again. Only have seen {numbersSeen.Count}.");
            }
        }
    }
}