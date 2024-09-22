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

    /// <summary>
    /// This holds a set of direct parsing tests.
    /// Given a set of scripting tokens, which are defined in the test,
    /// the description text should parse out to an expected value.
    /// </summary>
    public class ParsingLanguageTests : EditModeTestBase
    {
        public struct AssertEffectScriptResultsInTextValueSourceValue
        {
            public string EffectScript;
            public string ExpectedParsedValue;

            public AssertEffectScriptResultsInTextValueSourceValue(string effectScript, string expectedParsedValue)
            {
                this.EffectScript = effectScript;
                this.ExpectedParsedValue = expectedParsedValue;
            }

            public override string ToString()
            {
                return this.ExpectedParsedValue;
            }
        }

        public static List<AssertEffectScriptResultsInTextValueSourceValue> AssertEffectScriptResultsInTextValueSource => new List<AssertEffectScriptResultsInTextValueSourceValue>()
        {
            new AssertEffectScriptResultsInTextValueSourceValue("[SETTARGET: FOE][DAMAGE: 1]", "1 damage."),
            new AssertEffectScriptResultsInTextValueSourceValue("[SETTARGET: SELF][DAMAGE: 1]", "1 damage to self."),

            new AssertEffectScriptResultsInTextValueSourceValue("[SETTARGET: SELF][HEAL: 1]", "Heal 1."),
            new AssertEffectScriptResultsInTextValueSourceValue("[SETTARGET: FOE][HEAL: 1]", "Heal foe for 1."),

            new AssertEffectScriptResultsInTextValueSourceValue($"[SETTARGET: FOE][APPLYSTATUSEFFECTSTACKS: 1 {nameof(DebugStatus)}]", $"Apply 1 stack of {nameof(DebugStatus)} to foe."),
            new AssertEffectScriptResultsInTextValueSourceValue($"[SETTARGET: SELF][APPLYSTATUSEFFECTSTACKS: 2 {nameof(DebugStatus)}]", $"Apply 2 stacks of {nameof(DebugStatus)} to self."),

            new AssertEffectScriptResultsInTextValueSourceValue($"[SETTARGET: FOE][REMOVESTATUSEFFECTSTACKS: 1 {nameof(DebugStatus)}]", $"Remove 1 stack of {nameof(DebugStatus)} from foe."),
            new AssertEffectScriptResultsInTextValueSourceValue($"[SETTARGET: SELF][REMOVESTATUSEFFECTSTACKS: 2 {nameof(DebugStatus)}]", $"Remove 2 stacks of {nameof(DebugStatus)} from self."),


            new AssertEffectScriptResultsInTextValueSourceValue($"[SETTARGET: FOE][DAMAGE: COUNTSTACKS_{nameof(DebugStatus)}]", $"1 x {nameof(DebugStatus)} damage."),
        };

        [Test]
        public void AssertEffectScriptResultsInText([ValueSource(nameof(AssertEffectScriptResultsInTextValueSource))] AssertEffectScriptResultsInTextValueSourceValue expectations)
        {
            CardImport import = new CardImport()
            {
                Id = nameof(AssertEffectScriptResultsInText),
                Name = nameof(AssertEffectScriptResultsInText),
                EffectScript = expectations.EffectScript
            };

            Card derivedCard = new Card(import);
            EffectDescription description = derivedCard.GetDescription();

            List<string> descriptionTexts = description.DescriptionText;
            Assert.AreEqual(1, descriptionTexts.Count, "Scripts should only parse in to one description text when validated using this function.");
            Assert.AreEqual(expectations.ExpectedParsedValue, descriptionTexts[0], "Script should parse out to expected value.");
        }
    }
}
