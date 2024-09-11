namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public static class ScriptTokenEvaluator
    {
        public static GamestateDelta CalculateDifferenceFromTokenEvaluation(CentralGameStateController gameStateController, ICombatantTarget actor, IAttackTokenHolder evaluatedAttack, ICombatantTarget target)
        {
            GamestateDelta resultingDelta = new GamestateDelta();

            List<TokenEvaluatorBuilder> builders = CalculateEvaluatorBuildersFromTokenEvaluation(actor, evaluatedAttack, target: target);
            foreach (TokenEvaluatorBuilder builder in builders)
            {
                ApplyMissingDefaultInformation(builder, actor, gameStateController);

                if (builder.MeetsElementRequirements(gameStateController))
                {
                    resultingDelta.AppendDelta(builder.GetEffectiveDelta(gameStateController));
                }
            }

            return resultingDelta;
        }

        public static List<TokenEvaluatorBuilder> CalculateEvaluatorBuildersFromTokenEvaluation(ICombatantTarget actor, IAttackTokenHolder evaluatedAttack, ICombatantTarget target = null)
        {
            List<TokenEvaluatorBuilder> builders = new List<TokenEvaluatorBuilder>();
            TokenEvaluatorBuilder builder = new TokenEvaluatorBuilder();
            builder.User = actor;
            builder.Target = target;

            if (builder.Target == null)
            {
                builder.Target = new FoeTarget();
            }

            builder.TopOfEffectTarget = builder.Target;

            Dictionary<string, int> previousRequirements = new Dictionary<string, int>();

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
                        builder.ElementRequirements = new Dictionary<string, int>(previousRequirements);
                    }
                    builders.Add(builder);

                    previousRequirements = builder.ElementRequirements;

                    ICombatantTarget previousTarget = builder.Target;

                    builder = new TokenEvaluatorBuilder();
                    builder.User = actor;
                    builder.Target = previousTarget;
                    builder.TopOfEffectTarget = builder.Target;
                }
            }

            return builders;
        }

        public static void ApplyMissingDefaultInformation(TokenEvaluatorBuilder builder, ICombatantTarget actor, CentralGameStateController gameStateController)
        {
            if (builder.User == null)
            {
                builder.User = actor;
            }
            else if (builder.User is AbstractPlayerUser)
            {
                builder.User = gameStateController.CurrentPlayer;
            }

            if (builder.Target == null || builder.Target is FoeTarget)
            {
                if (builder.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
                {
                    if (actor is Player)
                    {
                        if (gameStateController.CurrentRoom.Enemies.Count > 0)
                        {
                            int randomIndex = UnityEngine.Random.Range(0, gameStateController.CurrentRoom.Enemies.Count);
                            builder.Target = gameStateController.CurrentRoom.Enemies[randomIndex];
                        }
                    }
                    else
                    {
                        builder.Target = gameStateController.CurrentPlayer;
                    }
                }
                else
                {
                    builder.Target = actor;
                }
            }
        }

        public static string DescribeCardText(Card importingCard)
        {
            List<TokenEvaluatorBuilder> builders = CalculateEvaluatorBuildersFromTokenEvaluation(new AbstractPlayerUser(), importingCard);
            StringBuilder effectText = new StringBuilder();

            // When parsing a card, if three abilities sequentially have the same elemental requirements,
            // don't display the redundant requirements, if the requirements are empty.
            string previousRequirements = string.Empty;

            foreach (TokenEvaluatorBuilder builder in builders)
            {
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

        public static string DescribeEnemyAttackIntent(CentralGameStateController centralGameStateController, Enemy user, EnemyAttack attack)
        {
            List<TokenEvaluatorBuilder> builders = CalculateEvaluatorBuildersFromTokenEvaluation(user, attack);
            StringBuilder effectText = new StringBuilder();

            foreach (TokenEvaluatorBuilder builder in builders)
            {
                GamestateDelta delta = builder.GetEffectiveDelta(centralGameStateController);

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

                if (currentToken is SetTargetSelfScriptingToken)
                {
                    return user == target;
                }
                else if (currentToken is SetTargetFoeScriptingToken)
                {
                    return user.IsFoeOf(target);
                }
            }

            // If no explicit tokens have been defend,
            // then go through each ability until we hit something with a target implied
            for (int ii = 0; ii < effect.AttackTokens.Count; ii++)
            {
                IScriptingToken currentToken = effect.AttackTokens[ii];

                if (currentToken.IsHarmfulToTarget(user, target) && user.IsFoeOf(target))
                {
                    return true;
                }
            }

            // If there's still no hits, then no this can't effect target
            return false;
        }
    }
}