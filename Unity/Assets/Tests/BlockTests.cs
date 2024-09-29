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


    public class BlockTests : EditModeTestBase
    {
        /// <summary>
        /// This test is meant to be a test for the Block status effect.
        /// Damage should be reduced by block before reducing health.
        /// This test validates the behaviour.
        /// </summary>
        [Test]
        public void TestBlock()
        {
            StatusEffectDatabase.AddStatusEffectToDatabase(ImportHelper.ImportImportableFile<StatusEffectImport>(Application.streamingAssetsPath + "/sets/fundamentals/statuseffect/block.statusImport"));
            StatusEffect blockStatus = StatusEffectDatabase.GetModel("block");

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(2, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            int startingHealth = campaignContext.CampaignPlayer.CurrentHealth;
            int numberOfBlockStacksToGivePlayer = 10;
            int damageToDealToPlayer = 5;
            int expectedRemainingBlock = numberOfBlockStacksToGivePlayer - damageToDealToPlayer;

            EditModeTestCommon.ApplyStatusEffectStacks(blockStatus.Id, campaignContext, combatContext, combatContext.CombatPlayer, numberOfBlockStacksToGivePlayer);
            GamestateDelta delta = new GamestateDelta()
            {
                DeltaEntries = new List<DeltaEntry>()
                 {
                    new DeltaEntry(campaignContext, combatContext.CombatPlayer, combatContext.CombatPlayer)
                    {
                        IntensityKindType = TokenEvaluatorBuilder.IntensityKind.Damage,
                        Intensity = damageToDealToPlayer
                    }
                 }
            };
            delta.ApplyDelta(campaignContext);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.AreEqual(startingHealth, campaignContext.CampaignPlayer.CurrentHealth, $"Because of the block, it is expected the player takes no damage.");
            Assert.AreEqual(expectedRemainingBlock, campaignContext.CampaignPlayer.AppliedStatusEffects[0].Stacks, $"The player is expected to have {expectedRemainingBlock} remaining block.");

            int damageToDealNext = 10;
            int targetHealth = startingHealth - (damageToDealNext - expectedRemainingBlock);

            delta = new GamestateDelta()
            {
                DeltaEntries = new List<DeltaEntry>()
                 {
                    new DeltaEntry(campaignContext, combatContext.CombatPlayer, combatContext.CombatPlayer)
                    {
                        IntensityKindType = TokenEvaluatorBuilder.IntensityKind.Damage,
                        Intensity = damageToDealNext
                    }
                 }
            };
            delta.ApplyDelta(campaignContext);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Assert.AreEqual(targetHealth, campaignContext.CampaignPlayer.CurrentHealth, $"Because of the block, it is expected the player takes a specific amount of reduced damage.");
            Assert.AreEqual(0, campaignContext.CampaignPlayer.AppliedStatusEffects.Count, "Expecting all of the block to have fallen off, leaving no status effects behind.");
        }
    }
}
