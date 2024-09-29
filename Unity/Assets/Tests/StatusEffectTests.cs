namespace SFDDCards.Tests.EditMode
{
    using NUnit.Framework;
    using SFDDCards.ImportModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;


    public class StatusEffectTests : EditModeTestBase
    {
        /// <summary>
        /// This test is meant to test one of the most simple status effects, Poison.
        /// It tries to load the actual poison status effect configuration, starts a combat context,
        /// applies poison to various targets, then passes turns.
        /// The test observes that poison proc'd appropriately and has the desired effects.
        /// </summary>
        [Test]
        public async void TestPoisonStatus()
        {
            StatusEffectDatabase.AddStatusEffectToDatabase(await ImportHelper.ImportImportableFileAsync<StatusEffectImport>(Application.streamingAssetsPath + "/statusImport/status effects/poison.statusImport"));
            StatusEffect poisonStatus = StatusEffectDatabase.GetModel("poison");

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(2, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            const int numberOfStacksToGivePlayer = 10;
            int player_oneturnofpoison_expectedhealth = combatContext.CombatPlayer.CurrentHealth - numberOfStacksToGivePlayer;
            int player_twoturnofpoison_expectedhealth = combatContext.CombatPlayer.CurrentHealth - numberOfStacksToGivePlayer - (numberOfStacksToGivePlayer - 1);
            EditModeTestCommon.ApplyStatusEffectStacks(poisonStatus.Id, campaignContext, combatContext, combatContext.CombatPlayer, numberOfStacksToGivePlayer);

            const int numberOfStacksToGiveEnemyOne = 10;
            int enemyone_oneturnofpoison_expectedhealth = combatContext.Enemies[0].CurrentHealth - numberOfStacksToGiveEnemyOne;
            int enemyone_twoturnofpoison_expectedhealth = combatContext.Enemies[0].CurrentHealth - numberOfStacksToGiveEnemyOne - (numberOfStacksToGiveEnemyOne - 1);
            EditModeTestCommon.ApplyStatusEffectStacks(poisonStatus.Id, campaignContext, combatContext, combatContext.Enemies[0], numberOfStacksToGiveEnemyOne);

            const int singleStackOnEnemyTwo = 1;
            int enemytwo_oneturnofpoison_expectedhealth = combatContext.Enemies[1].CurrentHealth - singleStackOnEnemyTwo;
            int enemytwo_twoturnofpoison_expectedhealth = enemytwo_oneturnofpoison_expectedhealth; // Expected that the last stack falls off, so they aren't hurt
            EditModeTestCommon.ApplyStatusEffectStacks(poisonStatus.Id, campaignContext, combatContext, combatContext.Enemies[1], singleStackOnEnemyTwo);

            // End the player's turn, making it go to the enemy. The controller will immediately make it the player's turn again.
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.EnemyTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.AreEqual(CombatContext.TurnStatus.PlayerTurn, combatContext.CurrentTurnStatus, "It should be the player's turn.");
            Assert.AreEqual(player_oneturnofpoison_expectedhealth, combatContext.CombatPlayer.CurrentHealth, "Player should be damaged equal to the number of stacks");
            Assert.AreEqual(enemyone_oneturnofpoison_expectedhealth, combatContext.Enemies[0].CurrentHealth, "Enemy One should be damaged equal to the number of stacks");
            Assert.AreEqual(enemytwo_oneturnofpoison_expectedhealth, combatContext.Enemies[1].CurrentHealth, "Enemy Two should be damaged equal to the number of stacks");
            Assert.AreEqual(numberOfStacksToGivePlayer - 1, combatContext.CombatPlayer.CountStacks(poisonStatus.Id), "Player should have one less stack of poison after it applies.");
            Assert.AreEqual(numberOfStacksToGiveEnemyOne - 1, combatContext.Enemies[0].CountStacks(poisonStatus.Id), "Enemy One should have one less stack of poison after it applies.");
            Assert.AreEqual(singleStackOnEnemyTwo - 1, combatContext.Enemies[1].CountStacks(poisonStatus.Id), "Enemy Two should have one less stack of poison after it applies.");

            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.EnemyTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.AreEqual(CombatContext.TurnStatus.PlayerTurn, combatContext.CurrentTurnStatus, "It should be the player's turn.");
            Assert.AreEqual(player_twoturnofpoison_expectedhealth, combatContext.CombatPlayer.CurrentHealth, "Player should be damaged equal to the number of stacks");
            Assert.AreEqual(enemyone_twoturnofpoison_expectedhealth, combatContext.Enemies[0].CurrentHealth, "Enemy One should be damaged equal to the number of stacks");
            Assert.AreEqual(enemytwo_twoturnofpoison_expectedhealth, combatContext.Enemies[1].CurrentHealth, "Enemy Two should be damaged equal to the number of stacks");
            Assert.AreEqual(numberOfStacksToGivePlayer - 2, combatContext.CombatPlayer.CountStacks(poisonStatus.Id), "Player should have two fewer stacks of poison after it applies.");
            Assert.AreEqual(numberOfStacksToGiveEnemyOne - 2, combatContext.Enemies[0].CountStacks(poisonStatus.Id), "Enemy One should have two fewer stacks of poison after it applies.");
            Assert.AreEqual(0, combatContext.Enemies[1].CountStacks(poisonStatus.Id), "Enemy Two's poison should have fallen off entirely.");
        }
    }
}
