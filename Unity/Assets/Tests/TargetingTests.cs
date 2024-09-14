namespace SFDDCards.Tests.EditMode
{
    using NUnit.Framework;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;


    public class TargetingTests
    {
        [TearDown]
        public void TearDown()
        {
            GlobalSequenceEventHolder.StopAllSequences();
        }

        /// <summary>
        /// Ensures that [SETTARGET: FOE] does not override an original, specific target.
        /// 
        /// ADDRESSING BUG: Character has multiple Foes. When specifying the attack, a
        /// specific target is chosen to attack. The attack contains a [SETTARGET: FOE] token,
        /// and it overrides the specific target to choose a random target instead.
        /// </summary>
        [Test]
        public void FoeTarget_DoesNotOverride_SpecificTarget()
        {
            // How many times to run the targeting before we're satisfied that it worked
            const int TimesToRunTest = 20;

            CardImport import = new CardImport()
            {
                Id = nameof(FoeTarget_DoesNotOverride_SpecificTarget),
                Name = nameof(FoeTarget_DoesNotOverride_SpecificTarget),
                EffectScript = "[SETTARGET: FOE][DAMAGE: 1]"
            };

            Encounter testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(10, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(testEncounter);
            CombatContext combatContext = campaignContext.CurrentCombatContext;

            for (int ii = 0; ii < TimesToRunTest; ii++)
            {
                combatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Add(import.DeriveCard());
            }

            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            EditModeTestCommon.PlayerDrawsDeck(combatContext);

            for (int ii = 0; ii < TimesToRunTest; ii++)
            {
                // Determine the theory of who would be the target of the effect, and ensure it is as predicted
                Enemy target = combatContext.Enemies[0];
                Card playedCard = combatContext.PlayerCombatDeck.CardsCurrentlyInHand[0];

                GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(combatContext.FromCampaign, combatContext.CombatPlayer, playedCard, target);
                Assert.AreEqual(target, delta.DeltaEntries[0].Target, $"The predicted target of the effect should be the first intentional target, not another one.");

                // Now actually play the card
                int previousHealth = target.CurrentHealth;
                combatContext.PlayCard(playedCard, combatContext.Enemies[0]);
                GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

                // The target should have one less health than before
                Assert.AreEqual(previousHealth - 1, target.CurrentHealth, $"The chosen target should have one less health.");

                // No one else should be affected
                Assert.AreEqual(combatContext.CombatPlayer.MaxHealth, combatContext.CombatPlayer.CurrentHealth, $"The player should have maximum health.");

                // Skip the first index, since that enemy should be our target
                for (int jj = 1; jj < combatContext.Enemies.Count; jj++)
                {
                    Assert.AreEqual(combatContext.Enemies[jj].MaxHealth, combatContext.Enemies[jj].CurrentHealth, $"Enemies not targeted should have maximum health.");
                }
            }
        }
    }
}
