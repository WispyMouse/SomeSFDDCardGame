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
        public struct AssertEffectScriptResultsValueSourceValue
        {
            public string EffectScript;
            public string ExpectedParsedValue;
            public string ReactionWindow;

            public AssertEffectScriptResultsValueSourceValue(string effectScript, string expectedParsedValue, string reactionWindow = "testwindow")
            {
                this.EffectScript = effectScript;
                this.ExpectedParsedValue = expectedParsedValue;
                this.ReactionWindow = reactionWindow;
            }

            public override string ToString()
            {
                return this.ExpectedParsedValue;
            }
        }

        public static List<AssertEffectScriptResultsValueSourceValue> AssertEffectScriptResultsInTextValueSource => new List<AssertEffectScriptResultsValueSourceValue>()
        {
            new AssertEffectScriptResultsValueSourceValue("[SETTARGET: FOE][DAMAGE: 1]", "1 damage."),
            new AssertEffectScriptResultsValueSourceValue("[SETTARGET: SELF][DAMAGE: 1]", "1 damage to self."),

            new AssertEffectScriptResultsValueSourceValue("[SETTARGET: SELF][HEAL: 1]", "Heal 1."),
            new AssertEffectScriptResultsValueSourceValue("[SETTARGET: FOE][HEAL: 1]", "Heal foe for 1."),

            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][APPLYSTATUSEFFECTSTACKS: 1 {nameof(DebugStatus)}]", $"Apply 1 stack of {nameof(DebugStatus)} to foe."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: SELF][APPLYSTATUSEFFECTSTACKS: 2 {nameof(DebugStatus)}]", $"Apply 2 stacks of {nameof(DebugStatus)} to self."),

            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][REMOVESTATUSEFFECTSTACKS: 1 {nameof(DebugStatus)}]", $"Remove 1 stack of {nameof(DebugStatus)} from foe."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: SELF][REMOVESTATUSEFFECTSTACKS: 2 {nameof(DebugStatus)}]", $"Remove 2 stacks of {nameof(DebugStatus)} from self."),

            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][SETSTATUSEFFECTSTACKS: 1 {nameof(DebugStatus)}]", $"Set {nameof(DebugStatus)} to 1 stack on foe."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: SELF][SETSTATUSEFFECTSTACKS: 2 {nameof(DebugStatus)}]", $"Set {nameof(DebugStatus)} to 2 stacks on self."),

            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][SETELEMENT: 1 DEBUGELEMENTONEID]", $"Set {DebugElementOneIconText}DEBUGELEMENTONENAME to 1."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: SELF][SETELEMENT: 0 DEBUGELEMENTONEID]", $"Set {DebugElementOneIconText}DEBUGELEMENTONENAME to 0."),

            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][DAMAGE: COUNTSTACKS_{nameof(DebugStatus)}]", $"1 x {nameof(DebugStatus)} damage."),

            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][REQUIRESATLEASTELEMENT: 2 DEBUGELEMENTONEID][DAMAGE: 3]", $"2 {DebugElementOneIconText}DEBUGELEMENTONENAME: 3 damage."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][REQUIRESATLEASTELEMENT: 2 DEBUGELEMENTONEID][REQUIRESATLEASTELEMENT: 5 DEBUGELEMENTTWOID][DAMAGE: 5][SETTARGET: SELF][HEAL: 7]", $"2 {DebugElementOneIconText}DEBUGELEMENTONENAME, 5 {DebugElementTwoIconText}DEBUGELEMENTTWONAME: 5 damage. Heal self for 7.")
        };

        [Test]
        public void AssertEffectScriptResultsInTextAsCard([ValueSource(nameof(AssertEffectScriptResultsInTextValueSource))] AssertEffectScriptResultsValueSourceValue expectations)
        {
            CardImport import = new CardImport()
            {
                Id = nameof(AssertEffectScriptResultsInTextAsCard),
                Name = nameof(AssertEffectScriptResultsInTextAsCard),
                EffectScript = expectations.EffectScript
            };

            Card derivedCard = new Card(import);
            EffectDescription description = derivedCard.GetDescription();

            List<string> descriptionTexts = description.DescriptionText;
            Assert.AreEqual(1, descriptionTexts.Count, "Scripts should only parse in to one description text when validated using this function.");
            Assert.AreEqual(expectations.ExpectedParsedValue, descriptionTexts[0], "Script should parse out to expected value.");
        }

        public static List<AssertEffectScriptResultsValueSourceValue> AssertEffectScriptResultsInTextAsStatusEffectValueSource => new List<AssertEffectScriptResultsValueSourceValue>()
        {
            new AssertEffectScriptResultsValueSourceValue("[REMOVESTACKS: 1]", $"Remove 1 stack."),
            new AssertEffectScriptResultsValueSourceValue("[IFTARGET: SELF][DRAINBOTH: INTENSITY DEBUGSTATUS]", $"<b>Incoming Damage:</b> Damage first subtracts from DEBUGSTATUS before subtracting from health.", KnownReactionWindows.IncomingDamage)
        };

        [Test]
        public void AssertEffectScriptResultsInTextAsStatusEffect([ValueSource(nameof(AssertEffectScriptResultsInTextAsStatusEffectValueSource))] AssertEffectScriptResultsValueSourceValue expectations)
        {
            AttackTokenPile pile = ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(expectations.EffectScript, this.DebugStatus);
            this.DebugStatus.EffectTokens.Clear();
            this.DebugStatus.EffectTokens.Add(expectations.ReactionWindow, new List<AttackTokenPile>() { pile });

            string resolvedDescription = this.DebugStatus.DescribeStatusEffect().BreakDescriptionsIntoString();
            Assert.AreEqual(expectations.ExpectedParsedValue, resolvedDescription, "Script should parse out to expected value.");
        }
    }
}
