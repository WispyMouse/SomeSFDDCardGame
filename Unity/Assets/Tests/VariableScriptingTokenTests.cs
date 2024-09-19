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
            Card derivedCard = new Card(import);
            Assert.AreEqual(1, derivedCard.AttackTokens.Count, $"Card should have one attack tokens.");
            Assert.IsTrue(derivedCard.AttackTokens[0] is LogIntScriptingToken, $"Attack token should be of type {typeof(LogIntScriptingToken).Name}.");

            LogIntScriptingToken token = derivedCard.AttackTokens[0] as LogIntScriptingToken;
            Assert.IsTrue(token.ValueToLog is ConstantEvaluatableValue<int>, $"Attack token should have a constant value. Is {token.ValueToLog.GetType()} instead.");

            ConstantEvaluatableValue<int> constant = token.ValueToLog as ConstantEvaluatableValue<int>;

            Assert.AreEqual(randomConstant, constant.ConstantValue, $"Parsed token should have the provided constant value.");

            TestVariableInCombatContext(derivedCard, constant, randomConstant);
        }

        [Test]
        public void CountStacks_CountsSelf_Correctly()
        {
            StatusEffectImport statusImport = new StatusEffectImport()
            {
                Id = nameof(statusImport),
                Name = nameof(statusImport),
                Effects = new List<EffectOnProcImport>()
            };

            StatusEffectDatabase.AddStatusEffectToDatabase(statusImport);

            CardImport selfCountStackCard = new CardImport()
            {
                Id = nameof(CountStacks_CountsSelf_Correctly),
                Name = nameof(CountStacks_CountsSelf_Correctly),
                EffectScript = $"[SETTARGET: SELF][LOGINT: COUNTSTACKS_{nameof(statusImport)}]"
            };

            // Derive the card to look at its tokens
            Card selfCountDerivedCard = new Card(selfCountStackCard);
            Assert.AreEqual(2, selfCountDerivedCard.AttackTokens.Count, $"Card should have one attack tokens.");
            Assert.IsTrue(selfCountDerivedCard.AttackTokens[1] is LogIntScriptingToken, $"Attack token should be of type {typeof(LogIntScriptingToken).Name}.");

            LogIntScriptingToken selfCountToken = selfCountDerivedCard.AttackTokens[1] as LogIntScriptingToken;
            Assert.IsTrue(selfCountToken.ValueToLog is CountStacksEvaluatableValue, $"Attack token should have a 'count stacks' type. Is {selfCountToken.ValueToLog.GetType()} instead.");

            CountStacksEvaluatableValue selfCountVariable = selfCountToken.ValueToLog as CountStacksEvaluatableValue;

            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;

            // Count the number of stacks, which should be zero on everyone now
            // The created token above should target the owner
            TestVariableInCombatContext(selfCountDerivedCard, selfCountVariable, 0, combatContext, combatContext.CombatPlayer, combatContext.Enemies[0]);

            // Give the player stacks of the status
            const int numberOfStacksToGivePlayer = 10;
            EditModeTestCommon.ApplyStatusEffectStacks(statusImport.Id, combatContext, combatContext.CombatPlayer, numberOfStacksToGivePlayer);
            TestVariableInCombatContext(selfCountDerivedCard, selfCountVariable, numberOfStacksToGivePlayer, combatContext, combatContext.CombatPlayer, combatContext.Enemies[0]);

            // Give the enemy some stacks of the status, which shouldn't affect the evaluation of the token
            const int numberOfStacksToGiveOpponent = 8;
            EditModeTestCommon.ApplyStatusEffectStacks(statusImport.Id, combatContext, combatContext.Enemies[0], numberOfStacksToGiveOpponent);
            TestVariableInCombatContext(selfCountDerivedCard, selfCountVariable, numberOfStacksToGivePlayer, combatContext, combatContext.CombatPlayer, combatContext.Enemies[0]);

            // Give the player some more stacks and make sure that updates
            const int numberOfStacksToGivePlayerNext = 18;
            EditModeTestCommon.ApplyStatusEffectStacks(statusImport.Id, combatContext, combatContext.CombatPlayer, numberOfStacksToGivePlayerNext);
            TestVariableInCombatContext(selfCountDerivedCard, selfCountVariable, numberOfStacksToGivePlayer + numberOfStacksToGivePlayerNext, combatContext, combatContext.CombatPlayer, combatContext.Enemies[0]);

            // Give the player a different status effect; it shouldn't change the evaluation
            StatusEffectImport otherStatus = new StatusEffectImport()
            {
                Id = nameof(otherStatus),
                Name = nameof(otherStatus),
                Effects = new List<EffectOnProcImport>()
            };
            StatusEffectDatabase.AddStatusEffectToDatabase(otherStatus);
            EditModeTestCommon.ApplyStatusEffectStacks(otherStatus.Id, combatContext, combatContext.CombatPlayer, 99);
            TestVariableInCombatContext(selfCountDerivedCard, selfCountVariable, numberOfStacksToGivePlayer + numberOfStacksToGivePlayerNext, combatContext, combatContext.CombatPlayer, combatContext.Enemies[0]);
        }

        public void TestVariableInCombatContext(IAttackTokenHolder attackHolderStack, IEvaluatableValue<int> variable, int expectedValue, int builderIndexContainingToken = 0)
        {
            EncounterModel testEncounter = EditModeTestCommon.GetEncounterWithPunchingBags(1, 100);
            CampaignContext campaignContext = EditModeTestCommon.GetBlankCampaignContext();
            campaignContext.StartNextRoomFromEncounter(new EvaluatedEncounter(testEncounter));
            CombatContext combatContext = campaignContext.CurrentCombatContext;
            TestVariableInCombatContext(attackHolderStack, variable, expectedValue, combatContext, combatContext.CombatPlayer, combatContext.Enemies[0], builderIndexContainingToken);
        }

        public void TestVariableInCombatContext(IAttackTokenHolder attackHolderStack, IEvaluatableValue<int> variable, int expectedValue, CombatContext context, ICombatantTarget user, ICombatantTarget target, int builderIndexContainingToken = 0)
        {
            List<TokenEvaluatorBuilder> builders = ScriptTokenEvaluator.CalculateEvaluatorBuildersFromTokenEvaluation(attackHolderStack, user, target);
            TokenEvaluatorBuilder builder = builders[builderIndexContainingToken];

            if (!variable.TryEvaluateValue(context.FromCampaign, builder, out int evaluatedValue))
            {
                Assert.Fail($"Value should be able to evaluate.");
            }

            Assert.AreEqual(expectedValue, evaluatedValue, $"Value should be as expected after evaluation.");
        }
    }
}