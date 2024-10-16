namespace SFDDCards.Tests.EditMode
{
    using NUnit.Framework;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using ScriptingTokens;
    using ScriptingTokens.EvaluatableValues;
    using SFDDCards.ImportModels;
    using static SFDDCards.Tests.EditMode.EditModeTestCommon;
    using UnityEngine.TestTools;

    public class Test_Set_MB1_Parsing
    {
        public static string RootPath => $"{Application.streamingAssetsPath}/sets/mb1/";
        public ReactionWindowContext? PlayedFromHandContext;

        public static DependentFile[] DependentFiles = new DependentFile[]
        {
            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/element/void", ParseKind.Element),
            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/element/solar", ParseKind.Element),
            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/element/force", ParseKind.Element),
            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/element/cyber", ParseKind.Element),
            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/element/bio", ParseKind.Element),

            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/statuseffect/block", ParseKind.StatusEffect)
        };

        public static ParseFromFileTestData[] ParsingTests = new ParseFromFileTestData[]
        {
            new ParseFromFileTestData("mb1_card_starter_strike", "3 + Force damage.\r\nIf target's health > 10: Clear Force.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_starter_block", "Gain 3 Block.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_starter_resonate", $"Exile this card.\r\nIf Cyber + Void + Solar + Bio + Force {RequiresComparisonScriptingToken.GreaterThanOrEqualToAscii} 10: Draw a card.", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_common_burnarecord", "Create 1 x Cyber Loot in hand. Exile this card.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_fueledbypassion", "Exile the top card of the deck. Draw 2 cards.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_glitch", $"2 damage to all foes.\r\nIf Cyber < 3: Exile this card.\r\nIf Cyber {RequiresComparisonScriptingToken.GreaterThanOrEqualToAscii} 3: Lose 3 Cyber. Return this card to hand.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_invigorate", "Gain 1 Bio Heal.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_laserblast", "2 damage. Apply 4 Targeted.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_radiantstrike", "Solar + Cyber damage.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_sneeze", "1 x Force damage to all foes. Apply 1 x Bio Poison.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_tuckandroll", "Gain 5 Block. Put a card from the top 3 cards of the deck in hand. Shuffle.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_voiddrink", $"2 damage to self.\r\nIf Void {RequiresComparisonScriptingToken.GreaterThanOrEqualToAscii} 2: Clear Void. Draw 2 cards. Exile this card.", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_uncommon_devotiontoacause", $"If Solar {RequiresComparisonScriptingToken.GreaterThanOrEqualToAscii} 5: Draw 3 cards. Exile a card from hand.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_uncommon_tomorrowstar", "Gain 2 Tomorrow Star.", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_rare_finaldawn", $"If Void + Solar {RequiresComparisonScriptingToken.GreaterThanOrEqualToAscii} 15: 40 damage to all foes. Clear Void. Clear Solar.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_rare_giantlaserfromspace", "Set Laser Calibrating Aim to 2 stacks on foe.", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_generated_loot", "Draw a card. Discard a card. Exile this card.", ParseKind.Card),

            new ParseFromFileTestData("mb1_statuseffect_tomorrowstar", "<b>Start of turn:</b> Gain 5 Solar. Remove 1 stack.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_regen", "<b>Start of turn:</b> Heal 1 x Regen. Remove 1 stack.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_poison", "<b>End of turn:</b> 1 x Poison damage to self. Remove 1 stack.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_lasercalibratingaim", "<b>Start of turn:</b> Remove 1 stack. If Laser Calibrating Aim = 0: 40 damage to self.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_delayeddraw", "<b>Start of turn:</b> Draw 1 x Delayed Draw card(s). Clear Delayed Draw.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_bioheal", "<b>End of turn:</b> Heal Bio x Bio Heal. Clear Bio Heal.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_targeted", "<b>Start of turn:</b> Clear Targeted.\r\n<b>Incoming Damage:</b> 1 x Targeted damage to self. Clear Targeted.", ParseKind.StatusEffect),
        };

        [OneTimeSetUp]
        public void ImportAllFiles()
        {
            LogAssert.ignoreFailingMessages = false;

            HashSet<string> alreadyImportedFiles = new HashSet<string>();

            foreach (DependentFile dependentFile in DependentFiles)
            {
                if (alreadyImportedFiles.Contains(dependentFile.TypelessFilepath))
                {
                    continue;
                }

                switch (dependentFile.ParseKind)
                {
                    case ParseKind.Card:
                        CardDatabase.AddCardToDatabase(ImportHelper.GetFile<CardImport>(dependentFile.TypelessFilepath + ".cardimport"));
                        break;
                    case ParseKind.StatusEffect:
                        StatusEffectDatabase.AddStatusEffectToDatabase(ImportHelper.GetFile<StatusEffectImport>(dependentFile.TypelessFilepath + ".statusimport"));
                        break;
                    case ParseKind.Element:
                        ElementDatabase.AddElement(ImportHelper.GetFile<ElementImport>(dependentFile.TypelessFilepath + ".elementimport"));
                        break;
                }

                alreadyImportedFiles.Add(dependentFile.TypelessFilepath);
            }

            foreach (ParseFromFileTestData parseTestCandidate in ParsingTests)
            {
                if (alreadyImportedFiles.Contains(parseTestCandidate.Id))
                {
                    continue;
                }

                switch (parseTestCandidate.ParseKind)
                {
                    case ParseKind.Card:
                        CardDatabase.AddCardToDatabase(ImportHelper.GetFile<CardImport>(RootPath + "card/" + parseTestCandidate.Id + ".cardimport"));
                        break;
                    case ParseKind.StatusEffect:
                        StatusEffectDatabase.AddStatusEffectToDatabase(ImportHelper.GetFile<StatusEffectImport>(RootPath + "statuseffect/" + parseTestCandidate.Id + ".statusimport"));
                        break;
                }

                alreadyImportedFiles.Add(parseTestCandidate.Id);
            }

            PlayedFromHandContext = new ReactionWindowContext()
            {
                TimingWindowId = KnownReactionWindows.ConsideringPlayingFromHand,
                PlayedFromZone = "hand"
            };
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            CardDatabase.ClearDatabase();
            ElementDatabase.ClearDatabase();
            StatusEffectDatabase.ClearDatabase();
        }

        [Test]
        public void TestParsing([ValueSource(nameof(ParsingTests))] ParseFromFileTestData testData)
        {
            switch (testData.ParseKind)
            {
                case ParseKind.Card:
                    EditModeTestCommon.AssertCardParsing(CardDatabase.GetModel(testData.Id), testData.ExpectedParsedValue, ReactionWindowContext.LookingNotPlayingContext);
                    break;
                case ParseKind.StatusEffect:
                    EditModeTestCommon.AssertStatusEffectParsing(StatusEffectDatabase.GetModel(testData.Id), testData.ExpectedParsedValue);
                    break;
            }
        }

        /// <summary>
        /// Addresses bug: Resonate missing "Exile this card.".
        /// </summary>
        [Test]
        public void Resonate_ParsingIncludesExile()
        {
            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Card resonate = CardDatabase.GetModel("mb1_card_starter_resonate");
            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(resonate);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            EffectDescription resonateDescription = resonate.GetDescription(new ReactionWindowContext(campaignContext, KnownReactionWindows.ConsideringPlayingFromHand, combatContext.CombatPlayer, new NoTarget(), "hand"));
            string description = resonateDescription.BreakDescriptionsIntoString();
            Debug.Log(description);
            Assert.IsTrue(description.Contains("Exile this card."), "Resonate should exile.");
        }

        /// <summary>
        /// Addresses bug: Requires comparison should print out the total of the comparison.
        /// </summary>
        [Test]
        public void Resonate_ParsingIncludesElementalTotal()
        {
            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Card resonate = CardDatabase.GetModel("mb1_card_starter_resonate");
            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(resonate);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            EffectDescription resonateDescription = resonate.GetDescription(new ReactionWindowContext(campaignContext, KnownReactionWindows.ConsideringPlayingFromHand, combatContext.CombatPlayer, new NoTarget(), "hand"));
            string description = resonateDescription.BreakDescriptionsIntoString();
            Debug.Log(description);
            Assert.IsTrue(description.Contains("If Cyber + Void + Solar + Bio + Force (0) ≥ 10: Draw a card."), "Total value of elementals should be output as part of comparison.");
        }

        /// <summary>
        /// Addresses bug: Fueled By Passion missing "Exile the top card of the deck."
        /// </summary>
        [Test]
        public void FueledByPassion_ExileTopCard()
        {
            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Card fueldByPassion = CardDatabase.GetModel("mb1_card_common_fueledbypassion");
            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(fueldByPassion);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            EffectDescription fueldByPassionDescription = fueldByPassion.GetDescription(new ReactionWindowContext(campaignContext, KnownReactionWindows.ConsideringPlayingFromHand, combatContext.CombatPlayer, new NoTarget(), "hand"));
            string description = fueldByPassionDescription.BreakDescriptionsIntoString();
            Debug.Log(description);
            Assert.IsTrue(description.Contains("Exile the top card of the deck."), "Description should contain expected text.");
        }

        /// <summary>
        /// Addresses bug: Strike missing "Clear force."
        /// </summary>
        [Test]
        public void Strike_ClearForce()
        {
            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Card strike = CardDatabase.GetModel("mb1_card_starter_strike");
            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(strike);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            EffectDescription strikeDescription = strike.GetDescription(new ReactionWindowContext(campaignContext, KnownReactionWindows.ConsideringPlayingFromHand, combatContext.CombatPlayer, new NoTarget(), "hand"));
            string description = strikeDescription.BreakDescriptionsIntoString();
            Debug.Log(description);
            Assert.IsTrue(description.Contains("Clear Force."), "Description should contain expected text.");
        }

        /// <summary>
        /// Addresses bug: Target health evaluatable not being printed.
        /// </summary>
        [Test]
        public void Strike_CalculateTargetHealth()
        {
            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Card strike = CardDatabase.GetModel("mb1_card_starter_strike");
            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(strike);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            EffectDescription strikeDescription = strike.GetDescription(new ReactionWindowContext(campaignContext, KnownReactionWindows.ConsideringPlayingFromHand, combatContext.CombatPlayer, new NoTarget(), "hand"));
            string description = strikeDescription.BreakDescriptionsIntoString();
            Debug.Log(description);

            // The target will have 100 - 3 => 97 health after the effect happens
            // so indicate the health they'll have when this check happens
            Assert.IsTrue(description.Contains("If target's health (97) > 10:"), "Description should contain expected text.");
        }

        /// <summary>
        /// Addresses bug: All of burn a record missing its text.
        /// </summary>
        [Test]
        public void BurnARecord_Parsing()
        {
            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            combatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            Card burnARecord = CardDatabase.GetModel("mb1_card_common_burnarecord");
            combatContext.PlayerCombatDeck.CardsCurrentlyInHand.Add(burnARecord);
            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();

            EffectDescription burnARecordDescription = burnARecord.GetDescription(new ReactionWindowContext(campaignContext, KnownReactionWindows.ConsideringPlayingFromHand, combatContext.CombatPlayer, new NoTarget(), "hand"));
            string description = burnARecordDescription.BreakDescriptionsIntoString();
            Debug.Log(description);

            Assert.IsTrue(description.Contains("Create 1 x Cyber (0) Loot in hand. Exile this card."), "Description should contain expected text.");
        }
    }
}