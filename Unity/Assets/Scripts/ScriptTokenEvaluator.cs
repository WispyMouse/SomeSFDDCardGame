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
                    resultingDelta.AppendDelta(builder.GetEffectiveDelta());
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

                GamestateDelta delta = builder.GetEffectiveDelta();

                string deltaText = delta.DescribeAsEffect();

                if (!string.IsNullOrEmpty(deltaText))
                {
                    effectText.AppendLine(deltaText);
                }
            }

            return effectText.ToString();
        }
    }
}