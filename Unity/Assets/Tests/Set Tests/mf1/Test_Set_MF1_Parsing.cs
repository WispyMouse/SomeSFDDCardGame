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

    public class Test_Set_MF1_Parsing
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
            new ParseFromFileTestData("mb1_card_starter_resonate", "Exile this card.\r\nIf Cyber + Void + Solar + Bio + Force > 10: Draw a card.", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_common_burnarecord", "Create 1 x Cyber Loot in hand. Exile this card.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_fueldbypassion", "Exile the top card of the deck. Draw 2 cards.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_glitch", "2 damage to all foes.\r\nIf Cyber < 3: Exile this card.\r\nIf Cyber >= 3: Lose 3 Cyber. Return this card to hand.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_invigorate", "Gain 1 Bio Heal.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_laserblast", "2 damage. Apply 4 Targeted.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_radiantstrike", "Solar + Cyber damage.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_sneeze", "1 x Force damage to all foes. Apply 1 x Bio Poison.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_tuckandroll", "Gain 5 Block. Put 1 card from the top 3 cards of the deck in hand. Shuffle.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_voiddrink", "2 damage to self.\r\nIf Void >= 2: Clear Void. Draw 2 cards. Exile this card.", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_uncommon_devotiontoacause", "If Solar >= 5: Draw 3 cards. Exile 1 card from hand.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_uncommon_tomorrowstar", "Gain 2 Tomorrow Star.", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_rare_finaldawn", "If Void + Solar >= 15: 40 damage to all foes. Clear Void. Clear Solar.", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_rare_giantlaserfromspace", "Set Laser Calibrating Aim to 2 stacks on foe.", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_generated_loot", "Draw a card. Discard a card. Exile this card.", ParseKind.Card),

            new ParseFromFileTestData("mb1_statuseffect_tomorrowstar", "<b>Start of turn:</b> Gain 5 Solar. Remove 1 stack.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_regen", "<b>End of turn:</b> Heal 1 x Regen to self. Remove 1 stack.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_poison", "<b>End of turn:</b> Take 1 x Poison damage. Remove 1 stack.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_lasercalibratingaim", "<b>Start of turn:</b> Remove 1 stack.\r\nIf Laser Calibrating Aim = 0: Take 40 damage.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_delayeddraw", "<b>Start of turn:</b> Draw 1 x Delayed Draw cards. Clear Delayed Draw.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_bioheal", "<b>End of turn:</b> Heal Bio x Bio Heal. Clear Bio Heal.", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_targeted", "<b>Start of turn:</b> Clear Targeted.\r\n<b>Incoming damage</b>: Take 1 x Targeted damage. Clear Targeted.", ParseKind.StatusEffect),
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
                    EditModeTestCommon.AssertCardParsing(CardDatabase.GetModel(testData.Id), testData.ExpectedParsedValue, PlayedFromHandContext);
                    break;
                case ParseKind.StatusEffect:
                    EditModeTestCommon.AssertStatusEffectParsing(StatusEffectDatabase.GetModel(testData.Id), testData.ExpectedParsedValue);
                    break;
            }
        }
    }
}