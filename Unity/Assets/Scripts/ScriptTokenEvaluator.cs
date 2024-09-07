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

            List<TokenEvaluatorBuilder> builders = CalculateEvaluatorBuildersFromTokenEvaluation(actor, evaluatedAttack);
            foreach (TokenEvaluatorBuilder builder in builders)
            {
                builder.Target = target;
                ApplyMissingDefaultInformation(builder, actor, gameStateController);
                resultingDelta.ApplyDelta(builder.GetEffectiveDelta());
            }

            return resultingDelta;
        }

        public static List<TokenEvaluatorBuilder> CalculateEvaluatorBuildersFromTokenEvaluation(ICombatantTarget actor, IAttackTokenHolder evaluatedAttack)
        {
            List<TokenEvaluatorBuilder> builders = new List<TokenEvaluatorBuilder>();
            TokenEvaluatorBuilder builder = new TokenEvaluatorBuilder();
            int finalIndex = evaluatedAttack.AttackTokens.Count - 1;
            for (int scriptIndex = 0; scriptIndex < evaluatedAttack.AttackTokens.Count; scriptIndex++)
            {
                IScriptingToken token = evaluatedAttack.AttackTokens[scriptIndex];
                token.ApplyToken(builder);
                builder.AppliedTokens.Add(token);

                if (scriptIndex == finalIndex || builder.ShouldLaunch)
                {
                    builders.Add(builder);
                    builder = new TokenEvaluatorBuilder();
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

            if (builder.Target == null)
            {
                if (builder.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
                {
                    builder.Target = gameStateController.CurrentPlayer;
                }
            }
        }

        public static string DescribeCardText(Card importingCard)
        {
            List<TokenEvaluatorBuilder> builders = CalculateEvaluatorBuildersFromTokenEvaluation(null, importingCard);
            StringBuilder effectText = new StringBuilder();

            foreach (TokenEvaluatorBuilder builder in builders)
            {
                GamestateDelta delta = builder.GetEffectiveDelta();
                effectText.AppendLine(delta.DescribeAsEffect());
            }

            return effectText.ToString();
        }
    }
}