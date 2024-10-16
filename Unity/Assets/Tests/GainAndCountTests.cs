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


    public class GainAndCountTests : EditModeTestBase
    {
        /// <summary>
        /// This test is meant to be a test for applying the effect of resource change immediately.
        /// 
        /// Addresses Bug: An ability gains element, and counts it immediately. It should be applied.
        /// </summary>
        [Test]
        public void TestGainAndCount()
        {
            CardImport import = new CardImport()
            {
                Id = nameof(TestGainAndCount),
                Name = nameof(TestGainAndCount),
                ElementGain = new List<ResourceGainImport>()
                {
                    new ResourceGainImport()
                    {
                        Element = DebugElementOne.Id,
                        Gain = 1
                    }
                },
                EffectScript = $"[SETTARGET: FOE][DAMAGE: COUNTELEMENT_{DebugElementOne.Id}]"
            };

            CardDatabase.AddCardToDatabase(import);
            Card testGainCard = CardDatabase.GetModel(import.Id);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(testGainCard);
            combatContext.PlayCard(testGainCard, combatContext.Enemies[0]);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.AreEqual(99, combatContext.Enemies[0].CurrentHealth, "The effect should have dealt exactly one damage.");
        }
    }
}
