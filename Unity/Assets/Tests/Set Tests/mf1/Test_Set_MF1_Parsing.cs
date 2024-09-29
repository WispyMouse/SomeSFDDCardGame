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
        public static string RootPath => $"{Application.streamingAssetsPath}/cardImport/mint_firstedition/";

        public static DependentFile[] DependentFiles = new DependentFile[]
        {

        };

        public static ParseFromFileTestData[] ParsingTests = new ParseFromFileTestData[]
        {
            new ParseFromFileTestData("mf1_card_starter_strike", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_starter_block", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_starter_resonate", "", ParseKind.Card),

            new ParseFromFileTestData("mf1_card_common_burnarecord", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_common_fueldbypassion", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_common_glitch", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_common_laserblast", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_common_radiantstrike", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_common_sneeze", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_common_tuckandroll", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_common_voiddrink", "", ParseKind.Card),

            new ParseFromFileTestData("mf1_card_uncommon_devotiontoacause", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_uncommon_tomorrowstar", "", ParseKind.Card),

            new ParseFromFileTestData("mf1_card_rare_finaldawn", "", ParseKind.Card),
            new ParseFromFileTestData("mf1_card_rare_giantlaserfromspace", "", ParseKind.Card),

            new ParseFromFileTestData("mf1_card_generated_loot", "", ParseKind.Card),

            new ParseFromFileTestData("mf1_statuseffect_tomorrowstar", "", ParseKind.StatusEffect),
            new ParseFromFileTestData("mf1_statuseffect_regen", "", ParseKind.StatusEffect),
            new ParseFromFileTestData("mf1_statuseffect_poison", "", ParseKind.StatusEffect),
            new ParseFromFileTestData("mf1_statuseffect_lasercalibratingaim", "", ParseKind.StatusEffect),
            new ParseFromFileTestData("essential_block", "", ParseKind.StatusEffect)
        };

        [OneTimeSetUp]
        public void ImportAllFiles()
        {
            LogAssert.ignoreFailingMessages = false;

            HashSet<string> alreadyImportedFiles = new HashSet<string>();

            foreach (DependentFile dependentFile in DependentFiles)
            {
                if (alreadyImportedFiles.Contains(dependentFile.Id))
                {
                    continue;
                }

                switch (dependentFile.ParseKind)
                {
                    case ParseKind.Card:
                        CardDatabase.AddCardToDatabase(ImportHelper.GetFile<CardImport>(RootPath + dependentFile.Id + ".cardimport"));
                        break;
                    case ParseKind.StatusEffect:
                        StatusEffectDatabase.AddStatusEffectToDatabase(ImportHelper.GetFile<StatusEffectImport>(RootPath + dependentFile.Id + ".statusimport"));
                        break;
                }

                alreadyImportedFiles.Add(dependentFile.Id);
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
                        CardDatabase.AddCardToDatabase(ImportHelper.GetFile<CardImport>(RootPath + parseTestCandidate.Id + ".cardimport"));
                        break;
                    case ParseKind.StatusEffect:
                        StatusEffectDatabase.AddStatusEffectToDatabase(ImportHelper.GetFile<StatusEffectImport>(RootPath + parseTestCandidate.Id + ".statusimport"));
                        break;
                }

                alreadyImportedFiles.Add(parseTestCandidate.Id);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            CardDatabase.ClearDatabase();
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