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


    public class HandContextParsingTests : EditModeTestBase
    {
        /// <summary>
        /// This is the first level of a series of context-parsing tests.
        /// In this level, the player has already gained a resource.
        /// The card needs to reflect that it knows the value of this at the time of it being in your hand.
        /// </summary>
        [Test]
        public void CountStacks_Parses()
        {
            const int stacksToGive = 6;

            CardImport import = new CardImport()
            {
                Id = nameof(CountStacks_Parses),
                Name = nameof(CountStacks_Parses),
                EffectScript = $"[SETTARGET: FOE][DAMAGE: COUNTELEMENT_{DebugElementOne.Id}]"
            };

            CardDatabase.AddCardToDatabase(import);
            Card testGainCard = CardDatabase.GetModel(import.Id);

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents(campaignContext);

            combatContext.ElementResourceCounts.Add(this.DebugElementOne, stacksToGive);

            ReactionWindowContext fromHandContext = new ReactionWindowContext(campaignContext, KnownReactionWindows.ConsideringPlayingFromHand, combatContext.CombatPlayer, playedFromZone: "hand", combatantTarget: combatContext.Enemies[0]);

            EditModeTestCommon.AssertCardParsing(testGainCard, $"1 x {DebugElementOneIconText}{DebugElementOne.Name} ({stacksToGive}) damage to {combatContext.Enemies[0].Name}.", fromHandContext);
        }
    }
}
