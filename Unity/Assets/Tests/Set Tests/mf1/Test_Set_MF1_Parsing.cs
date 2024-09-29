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

    public class Test_Set_MF1_Parsing
    {
        public ParseFromFileTestData[] ParsingTests = new ParseFromFileTestData[]
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

        }

        [Test]
        public void TestParsing([ValueSource(nameof(ParsingTests))] ParseFromFileTestData testData)
        {

        }
    }
}