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
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][REQUIRESATLEASTELEMENT: 2 DEBUGELEMENTONEID][REQUIRESATLEASTELEMENT: 5 DEBUGELEMENTTWOID][DAMAGE: 5][SETTARGET: SELF][HEAL: 7]", $"2 {DebugElementOneIconText}DEBUGELEMENTONENAME, 5 {DebugElementTwoIconText}DEBUGELEMENTTWONAME: 5 damage. Heal self for 7."),

            new AssertEffectScriptResultsValueSourceValue($"[DRAW: 1][CARDTARGET: HAND][CHOOSECARDS: 1][MOVECARDTOZONE: DISCARD]", "Draw 1 card. Discard 1 card."),
            new AssertEffectScriptResultsValueSourceValue($"[DRAW: 2][CARDTARGET: HAND][CHOOSECARDS: 2][MOVECARDTOZONE: DISCARD]", "Draw 2 cards. Discard 2 cards."),

            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][DAMAGE: 1][MOVECARDTOZONE: EXILE]", "1 damage. Exile this card."),

            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][DAMAGE: 1+1]", "2 damage."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][DAMAGE: 2*3]", "6 damage."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][DAMAGE: 9/3]", "3 damage."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][DAMAGE: 4-3]", "1 damage."),

            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][DAMAGE: 1+COUNTELEMENT_DEBUGELEMENTONEID]", "1 + amount of DEBUGELEMENTONEID damage."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][DAMAGE: 2*COUNTELEMENT_DEBUGELEMENTONEID]", "2 * amount of DEBUGELEMENTONEID damage."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][DAMAGE: 3/COUNTELEMENT_DEBUGELEMENTONEID]", "3 / amount of DEBUGELEMENTONEID damage."),
            new AssertEffectScriptResultsValueSourceValue($"[SETTARGET: FOE][DAMAGE: 4-COUNTELEMENT_DEBUGELEMENTONEID]", "4 - amount of DEBUGELEMENTONEID damage."),
        };

        [Test]
        public void AssertEffectScriptResultsInTextAsCard([ValueSource(nameof(AssertEffectScriptResultsInTextValueSource))] AssertEffectScriptResultsValueSourceValue expectations)
        {
            EditModeTestCommon.AssertCardParsing(expectations.EffectScript, expectations.ExpectedParsedValue, this.PlayedFromHandContext);
        }

        public static List<AssertEffectScriptResultsValueSourceValue> AssertEffectScriptResultsInTextAsStatusEffectValueSource => new List<AssertEffectScriptResultsValueSourceValue>()
        {
            new AssertEffectScriptResultsValueSourceValue("[REMOVESTACKS: 1]", $"Remove 1 stack."),
            new AssertEffectScriptResultsValueSourceValue("[IFTARGET: SELF][DRAINBOTH: INTENSITY DEBUGSTATUS]", $"<b>Incoming Damage:</b> Damage first subtracts from DEBUGSTATUS before subtracting from health.", KnownReactionWindows.IncomingDamage)
        };

        [Test]
        public void AssertEffectScriptResultsInTextAsStatusEffect([ValueSource(nameof(AssertEffectScriptResultsInTextAsStatusEffectValueSource))] AssertEffectScriptResultsValueSourceValue expectations)
        {
            EditModeTestCommon.AssertStatusEffectParsing(expectations.EffectScript, expectations.ExpectedParsedValue, expectations.ReactionWindow, this.DebugStatus);
        }
    }
}
