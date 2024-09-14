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

    public class VariableScriptingTokenTests : EditModeTestBase
    {
        /// <summary>
        /// Generates random constant values in a scripting token.
        /// Those values should be parsed as a constant.
        /// Then when the effect is resolved, it should be the provided value.
        /// </summary>
        [Test]
        [Repeat(10)]
        public void ConstantValue_EvaluatesAsConstant()
        {
            int randomConstant = UnityEngine.Random.Range(-99999, 999999);

            CardImport import = new CardImport()
            {
                Id = nameof(ConstantValue_EvaluatesAsConstant),
                Name = nameof(ConstantValue_EvaluatesAsConstant),
                EffectScript = $"[LOGINT: {randomConstant.ToString()}]"
            };

            // Derive the card to look at its tokens
            Card derivedCard = import.DeriveCard();
            Assert.AreEqual(1, derivedCard.AttackTokens.Count, $"Card should have one attack tokens.");
            Assert.IsTrue(derivedCard.AttackTokens[0] is LogIntScriptingToken, $"Attack token should be of type {typeof(LogIntScriptingToken).Name}.");

            LogIntScriptingToken token = derivedCard.AttackTokens[0] as LogIntScriptingToken;
            Assert.IsTrue(token.ValueToLog is ConstantEvaluatableValue<int>, $"Second attack token should have a constant value. Is {token.ValueToLog.GetType()} instead.");

            ConstantEvaluatableValue<int> constant = token.ValueToLog as ConstantEvaluatableValue<int>;

            Assert.AreEqual(randomConstant, constant.ConstantValue, $"Parsed token should have the provided constant value.");
        }
    }
}