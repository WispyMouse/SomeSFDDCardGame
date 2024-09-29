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

        public static DependentFile[] DependentFiles = new DependentFile[]
        {
            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/element/void", ParseKind.Element),
            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/element/solar", ParseKind.Element),
            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/element/force", ParseKind.Element),
            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/element/cyber", ParseKind.Element),
            new DependentFile($"{Application.streamingAssetsPath}/sets/fundamentals/element/bio", ParseKind.Element)
        };

        public static ParseFromFileTestData[] ParsingTests = new ParseFromFileTestData[]
        {
            new ParseFromFileTestData("mb1_card_starter_strike", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_starter_block", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_starter_resonate", "", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_common_burnarecord", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_fueldbypassion", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_glitch", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_laserblast", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_radiantstrike", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_sneeze", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_tuckandroll", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_common_voiddrink", "", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_uncommon_devotiontoacause", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_uncommon_tomorrowstar", "", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_rare_finaldawn", "", ParseKind.Card),
            new ParseFromFileTestData("mb1_card_rare_giantlaserfromspace", "", ParseKind.Card),

            new ParseFromFileTestData("mb1_card_generated_loot", "", ParseKind.Card),

            new ParseFromFileTestData("mb1_statuseffect_tomorrowstar", "", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_regen", "", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_poison", "", ParseKind.StatusEffect),
            new ParseFromFileTestData("mb1_statuseffect_lasercalibratingaim", "", ParseKind.StatusEffect)
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
                    EditModeTestCommon.AssertCardParsing(CardDatabase.GetModel(testData.Id), testData.ExpectedParsedValue);
                    break;
                case ParseKind.StatusEffect:
                    EditModeTestCommon.AssertStatusEffectParsing(StatusEffectDatabase.GetModel(testData.Id), testData.ExpectedParsedValue);
                    break;
            }
        }
    }
}