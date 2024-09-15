namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public static class ScriptTokenEvaluator
    {
        public static GamestateDelta CalculateDifferenceFromTokenEvaluation(CampaignContext campaignContext, ICombatantTarget actor, IAttackTokenHolder evaluatedAttack, ICombatantTarget target)
        {
            GamestateDelta resultingDelta = new GamestateDelta();

            List<TokenEvaluatorBuilder> builders = CalculateEvaluatorBuildersFromTokenEvaluation(evaluatedAttack, actor, target);

            foreach (TokenEvaluatorBuilder builder in builders)
            {
                if (builder.MeetsElementRequirements(campaignContext.CurrentCombatContext) && builder.MeetsComparisonRequirements(campaignContext.CurrentCombatContext))
                {
                    resultingDelta.AppendDelta(builder.GetEffectiveDelta(campaignContext));
                }
            }

            resultingDelta.EvaluateVariables(campaignContext, actor, target);

            return resultingDelta;
        }

        public static List<TokenEvaluatorBuilder> CalculateEvaluatorBuildersFromTokenEvaluation(IAttackTokenHolder evaluatedAttack, ICombatantTarget actor = null, ICombatantTarget target = null)
        {
            List<TokenEvaluatorBuilder> builders = new List<TokenEvaluatorBuilder>();
            TokenEvaluatorBuilder builder = new TokenEvaluatorBuilder(actor, target);

            foreach (Element baseElement in evaluatedAttack.BaseElementGain.Keys)
            {
                builder.ElementResourceChanges.Add(new ElementResourceChange() { Element = baseElement, GainOrLoss = new ConstantEvaluatableValue<int>(evaluatedAttack.BaseElementGain[baseElement]) });
            }

            if (actor != null)
            {
                builder.User = actor;
            }
            else
            {
                actor = new NoTarget();
                builder.User = actor;
            }
            
            if (target != null)
            {
                builder.OriginalTarget = target;
                builder.Target = new SpecificTargetEvaluatableValue(target);
            }
            else
            {
                target = new NoTarget();
                builder.OriginalTarget = target;
            }

            if (builder.ElementResourceChanges.Count > 0)
            {
                builder.ShouldLaunch = true;
                builders.Add(builder);
                builder = new TokenEvaluatorBuilder(actor, target);
            }

            Dictionary<Element, IEvaluatableValue<int>> previousRequirements = new Dictionary<Element, IEvaluatableValue<int>>();

            int finalIndex = evaluatedAttack.AttackTokens.Count - 1;
            for (int scriptIndex = 0; scriptIndex < evaluatedAttack.AttackTokens.Count; scriptIndex++)
            {
                IScriptingToken token = evaluatedAttack.AttackTokens[scriptIndex];
                token.ApplyToken(builder);
                builder.AppliedTokens.Add(token);

                if (scriptIndex == finalIndex || builder.ShouldLaunch)
                {
                    // If the next ability set has no requirements tokens,
                    // re-use the previous set.
                    // This will allow for an easier set up of "if you meet this element criteria, play the rest of the card"
                    if (builder.ElementRequirements.Count == 0 && previousRequirements.Count != 0)
                    {
                        builder.ElementRequirements = new Dictionary<Element, IEvaluatableValue<int>>(previousRequirements);
                    }
                    builders.Add(builder);

                    previousRequirements = builder.ElementRequirements;

                    builder = TokenEvaluatorBuilder.Continue(builder);
                }
            }

            return builders;
        }

        public static string DescribeCardText(Card importingCard)
        {
            List<TokenEvaluatorBuilder> builders = CalculateEvaluatorBuildersFromTokenEvaluation(importingCard);
            StringBuilder effectText = new StringBuilder();

            // When parsing a card, if three abilities sequentially have the same elemental requirements,
            // don't display the redundant requirements, if the requirements are empty.
            string previousRequirements = string.Empty;

            foreach (TokenEvaluatorBuilder builder in builders)
            {
                // If this builder is just a constant resource gain in one or more categories, skip it for generating card text
                if (builder.ElementResourceChanges.Count != 0)
                {
                    bool nonConstantFound = false;

                    foreach (ElementResourceChange change in builder.ElementResourceChanges)
                    {
                        if (change.GainOrLoss is ConstantEvaluatableValue<int> constantEvaluatable)
                        {
                            continue;
                        }

                        nonConstantFound = true;
                        break;
                    }

                    if (!nonConstantFound)
                    {
                        continue;
                    }
                }

                string currentRequirements = builder.DescribeElementRequirements();

                if (string.IsNullOrEmpty(currentRequirements) && !string.IsNullOrEmpty(previousRequirements))
                {
                    // If there are no requirements, but the previous builder had requirements, notate that
                    // Don't do that if this is the first thing, or the previous requirements were also empty
                    effectText.AppendLine("(no requirements):");
                }
                else if (!string.IsNullOrEmpty(currentRequirements) && previousRequirements != currentRequirements)
                {
                    effectText.AppendLine(currentRequirements);
                }

                previousRequirements = currentRequirements;

                GamestateDelta delta = builder.GetAbstractDelta();

                string deltaText = delta.DescribeAsEffect();

                if (!string.IsNullOrEmpty(deltaText))
                {
                    effectText.AppendLine(deltaText);
                }
            }

            return effectText.ToString();
        }

        public static string DescribeEnemyAttackIntent(EnemyAttack attack)
        {
            List<TokenEvaluatorBuilder> builders = CalculateEvaluatorBuildersFromTokenEvaluation(attack);
            StringBuilder effectText = new StringBuilder();

            foreach (TokenEvaluatorBuilder builder in builders)
            {
                GamestateDelta delta = builder.GetAbstractDelta();

                string deltaText = delta.DescribeAsEffect();

                if (!string.IsNullOrEmpty(deltaText))
                {
                    effectText.AppendLine(deltaText);
                }
            }

            return effectText.ToString();
        }

        public static List<ICombatantTarget> GetTargetsThatCanBeTargeted(ICombatantTarget user, IAttackTokenHolder effect, List<ICombatantTarget> consideredTargets)
        {
            // Hunt until we find the first targeting token
            // If none is found, make assumptions based on the qualities of the ability
            // If no targets are found, that's still acceptable
            List<ICombatantTarget> validTargets = new List<ICombatantTarget>();

            foreach (ICombatantTarget currentConsideredTarget in consideredTargets)
            {
                if (CanEffectTarget(user, effect, currentConsideredTarget))
                {
                    validTargets.Add(currentConsideredTarget);
                }
            }

            return validTargets;
        }

        public static bool CanEffectTarget(ICombatantTarget user, IAttackTokenHolder effect, ICombatantTarget target)
        {
            // First pass through looking for all explicit targeting tokens
            for (int ii = 0; ii < effect.AttackTokens.Count; ii++)
            {
                IScriptingToken currentToken = effect.AttackTokens[ii];

                if (currentToken is SetTargetScriptingToken setTargetToken)
                {
                    if (setTargetToken.Target is SelfTargetEvaluatableValue)
                    {
                        return user == target;
                    }
                    else if (setTargetToken.Target is FoeTargetEvaluatableValue)
                    {
                        return user.IsFoeOf(target);
                    }
                    else if (setTargetToken.Target is AllFoeTargetEvaluatableValue)
                    {
                        return target is AllFoesTarget;
                    }
                }
            }

            // If no explicit tokens have been defend,
            // then go through each ability until we hit something with a target implied
            for (int ii = 0; ii < effect.AttackTokens.Count; ii++)
            {
                IScriptingToken currentToken = effect.AttackTokens[ii];

                if (currentToken.IsHarmfulToTarget(user, target) && user.IsFoeOf(target) && !(target is AllFoesTarget))
                {
                    return true;
                }
            }

            // If there's still no hits, then no this can't effect target
            return false;
        }
    }
}