namespace SFDDCards.Tests.EditMode
{
    using NUnit.Framework;
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;


    public class Test_Set_MB1_StatusEffects : EditModeTestBase
    {
        [Test]
        public void TestBurn_EnemyAction_Single()
        {
            StatusEffectDatabase.AddStatusEffectToDatabase(ImportHelper.ImportImportableFile<StatusEffectImport>(Application.streamingAssetsPath + "/sets/mb1/statuseffect/mb1_statuseffect_burn.statusImport"));
            StatusEffect burnStatus = StatusEffectDatabase.GetModel("burn");

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            int numberOfBurnStacksToGiveEnemy = 10;

            EditModeTestCommon.ApplyStatusEffectStacks(burnStatus.Id, campaignContext, combatContext, combatContext.Enemies[0], numberOfBurnStacksToGiveEnemy);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            combatContext.Enemies[0].Intent = new EnemyAttack(new EnemyAttackImport()
            {
                 AttackScript = "[SETTARGET: FOE][DAMAGE: 1]"
            });
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.EnemyTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.AreEqual(1, combatContext.Enemies[0].AppliedStatusEffects.Count, "Enemy should have one status on them.");
            Assert.AreEqual(numberOfBurnStacksToGiveEnemy - 1, combatContext.Enemies[0].AppliedStatusEffects[0].Stacks, "Enemy should have one fewer stack of burn after ending the turn.");
            Assert.AreEqual(combatContext.Enemies[0].MaxHealth - numberOfBurnStacksToGiveEnemy, combatContext.Enemies[0].CurrentHealth, "After one attack with 10 burn stacks, the enemy should have taken 10 damage.");
            Assert.AreEqual(combatContext.CombatPlayer.MaxHealth - 1, combatContext.CombatPlayer.CurrentHealth, $"The enemy attack should have dealt one damage to the player.");
        }

        [Test]
        public void TestBurn_EnemyAction_MultipleHits()
        {
            StatusEffectDatabase.AddStatusEffectToDatabase(ImportHelper.ImportImportableFile<StatusEffectImport>(Application.streamingAssetsPath + "/sets/mb1/statuseffect/mb1_statuseffect_burn.statusImport"));
            StatusEffect burnStatus = StatusEffectDatabase.GetModel("burn");

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            int numberOfBurnStacksToGiveEnemy = 10;

            EditModeTestCommon.ApplyStatusEffectStacks(burnStatus.Id, campaignContext, combatContext, combatContext.Enemies[0], numberOfBurnStacksToGiveEnemy);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            combatContext.Enemies[0].Intent = new EnemyAttack(new EnemyAttackImport()
            {
                AttackScript = "[SETTARGET: FOE][DAMAGE: 1][DAMAGE: 1][DAMAGE: 1]"
            });
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.EnemyTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.AreEqual(1, combatContext.Enemies[0].AppliedStatusEffects.Count, "Enemy should have one status on them.");
            Assert.AreEqual(numberOfBurnStacksToGiveEnemy - 1, combatContext.Enemies[0].AppliedStatusEffects[0].Stacks, "Enemy should have one fewer stack of burn after ending the turn."); 
            Assert.AreEqual(combatContext.Enemies[0].MaxHealth - numberOfBurnStacksToGiveEnemy * 3, combatContext.Enemies[0].CurrentHealth, "After three attack with 10 burn stacks, the enemy should have taken 30 damage.");
            Assert.AreEqual(combatContext.CombatPlayer.MaxHealth - 1, combatContext.CombatPlayer.CurrentHealth, $"The enemy attack should have dealt one damage to the player.");
        }

        [Test]
        public void TestBurn_EnemyAction_NoDamage()
        {
            StatusEffectDatabase.AddStatusEffectToDatabase(ImportHelper.ImportImportableFile<StatusEffectImport>(Application.streamingAssetsPath + "/sets/mb1/statuseffect/mb1_statuseffect_burn.statusImport"));
            StatusEffect burnStatus = StatusEffectDatabase.GetModel("burn");

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            int numberOfBurnStacksToGiveEnemy = 10;

            EditModeTestCommon.ApplyStatusEffectStacks(burnStatus.Id, campaignContext, combatContext, combatContext.Enemies[0], numberOfBurnStacksToGiveEnemy);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            combatContext.Enemies[0].Intent = new EnemyAttack(new EnemyAttackImport()
            {
                AttackScript = "[SETTARGET: FOE][DAMAGE: 0]"
            });
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.EnemyTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.AreEqual(1, combatContext.Enemies[0].AppliedStatusEffects.Count, "Enemy should have one status on them.");
            Assert.AreEqual(numberOfBurnStacksToGiveEnemy - 1, combatContext.Enemies[0].AppliedStatusEffects[0].Stacks, "Enemy should have one fewer stack of burn after ending the turn.");
            Assert.AreEqual(combatContext.Enemies[0].MaxHealth, combatContext.Enemies[0].CurrentHealth, "The attack should not have done any damage, so no burn procs should damage the enemy.");
            Assert.AreEqual(combatContext.CombatPlayer.MaxHealth, combatContext.CombatPlayer.CurrentHealth, $"The enemy attack should have dealt zero damage to the player.");
        }

        [Test]
        public void TestBurn_PlayerCard_Single()
        {
            CardImport damageImport = new CardImport()
            {
                Id = nameof(damageImport),
                Name = nameof(damageImport),
                EffectScript = "[SETTARGET: FOE][DAMAGE: 1]"
            };
            CardDatabase.AddCardToDatabase(damageImport);
            Card derivedCard = CardDatabase.GetModel(damageImport.Id);

            StatusEffectDatabase.AddStatusEffectToDatabase(ImportHelper.ImportImportableFile<StatusEffectImport>(Application.streamingAssetsPath + "/sets/mb1/statuseffect/mb1_statuseffect_burn.statusImport"));
            StatusEffect burnStatus = StatusEffectDatabase.GetModel("burn");

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            int numberOfBurnStacksToGivePlayer = 10;

            EditModeTestCommon.ApplyStatusEffectStacks(burnStatus.Id, campaignContext, combatContext, combatContext.CombatPlayer, numberOfBurnStacksToGivePlayer);
            combatContext.PlayerCombatDeck.MoveCardToZone(derivedCard, combatContext.PlayerCombatDeck.CardsCurrentlyInHand);
            combatContext.PlayCard(derivedCard, combatContext.Enemies[0]);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.AreEqual(combatContext.CombatPlayer.MaxHealth - numberOfBurnStacksToGivePlayer, combatContext.Enemies[0].CurrentHealth, "After one attack with 10 burn stacks, the player should have taken 10 damage.");
        }
    }
}
