namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
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
        public static List<ConceptualTokenEvaluatorBuilder> CalculateConceptualBuildersFromTokenEvaluation(IAttackTokenHolder evaluatedAttack, ReactionWindowContext? context = null)
        {
            List<ConceptualTokenEvaluatorBuilder> builders = new List<ConceptualTokenEvaluatorBuilder>();
            ConceptualTokenEvaluatorBuilder builder = new ConceptualTokenEvaluatorBuilder(context)
            {
                Owner = evaluatedAttack.Owner
            };

            foreach (Element baseElement in evaluatedAttack.BaseElementGain.Keys)
            {
                builder.ElementResourceChanges.Add(new ElementResourceChange() { Element = baseElement, GainOrLoss = new ConstantEvaluatableValue<int>(evaluatedAttack.BaseElementGain[baseElement]) });
            }

            if (builder.ElementResourceChanges.Count > 0)
            {
                builders.Add(builder);
                builder = new ConceptualTokenEvaluatorBuilder(builder);
            }

            int finalIndex = evaluatedAttack.AttackTokens.Count - 1;
            for (int scriptIndex = 0; scriptIndex < evaluatedAttack.AttackTokens.Count; scriptIndex++)
            {
                IScriptingToken token = evaluatedAttack.AttackTokens[scriptIndex];
                token.ApplyToken(builder);
                builder.AppliedTokens.Add(token);

                // If this is the last index in the builders, launch it now.
                // Launch it if there is any value for Intensity, as well; everything that applies intensity implies separate action
                if (builder.Intensity != null || scriptIndex == finalIndex || builder.RealizedOperationScriptingToken != null)
                {
                    builders.Add(builder);
                    builder = new ConceptualTokenEvaluatorBuilder(builder);
                }
            }

            return builders;
        }

        public static GamestateDelta CalculateRealizedDeltaEvaluation(IAttackTokenHolder evaluatedAttack, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget originalTarget, ReactionWindowContext? context = null)
        {
            List<ConceptualTokenEvaluatorBuilder> concepts = CalculateConceptualBuildersFromTokenEvaluation(evaluatedAttack, context);
            return RealizeConceptualBuilders(concepts, campaignContext, owner, user, originalTarget);
        }

        public static GamestateDelta RealizeConceptualBuilders(List<ConceptualTokenEvaluatorBuilder> concepts, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget originalTarget)
        {
            GamestateDelta resultingDelta = new GamestateDelta();
            TokenEvaluatorBuilder previousBuilder = null;

            foreach (ConceptualTokenEvaluatorBuilder conceptBuilder in concepts)
            {
                TokenEvaluatorBuilder realizedBuilder = RealizeConceptualBuilder(conceptBuilder, campaignContext, owner, user, originalTarget, previousBuilder);
                if (realizedBuilder.MeetsElementRequirements(campaignContext.CurrentCombatContext) && realizedBuilder.MeetsRequirements(campaignContext.CurrentCombatContext))
                {
                    resultingDelta.AppendDelta(realizedBuilder.GetEffectiveDelta(campaignContext));
                }
            }

            return resultingDelta;
        }

        public static TokenEvaluatorBuilder RealizeConceptualBuilder(ConceptualTokenEvaluatorBuilder concept, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget originalTarget, TokenEvaluatorBuilder previousBuilder = null)
        {
            return new TokenEvaluatorBuilder(concept, campaignContext, owner, user, originalTarget, previousBuilder);
        }

        public static GamestateDelta GetDeltaFromTokens(string attackTokens, CampaignContext context, Combatant user, ICombatantTarget target)
        {
            IAttackTokenHolder pile = ScriptingTokenDatabase.GetAllTokens(attackTokens, user);
            GamestateDelta delta = CalculateRealizedDeltaEvaluation(
                    pile,
                    context,
                    user,
                    user,
                    target);

            return delta;
        }

        public static string DescribeCardText(Card importingCard)
        {
            List<ConceptualTokenEvaluatorBuilder> builders = CalculateConceptualBuildersFromTokenEvaluation(importingCard);
            StringBuilder effectText = new StringBuilder();
            ConceptualTokenEvaluatorBuilder previousBuilder = null;

            foreach (ConceptualTokenEvaluatorBuilder builder in builders)
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

                // When parsing a card, if three abilities sequentially have the same elemental requirements,
                // don't display the redundant requirements, if the requirements are empty.
                if (previousBuilder == null
                    || !previousBuilder.HasSameElementRequirement(builder))
                {
                    string currentRequirements = builder.DescribeElementRequirements();

                    if (string.IsNullOrEmpty(currentRequirements))
                    {
                        if (previousBuilder != null)
                        {
                            // If there are no requirements, but the previous builder had requirements, notate that
                            // Don't do that if this is the first thing, or the previous requirements were also empty
                            effectText.Append("(no requirements):");
                        }
                    }
                    else if (!string.IsNullOrEmpty(currentRequirements))
                    {
                        effectText.Append(currentRequirements.Trim());
                    }
                }

                ConceptualDelta delta = builder.GetConceptualDelta();

                string deltaText = EffectDescriberDatabase.DescribeConceptualEffect(delta);

                if (!string.IsNullOrEmpty(deltaText))
                {
                    string leadingSpace = "";

                    if (effectText.Length > 0)
                    {
                        leadingSpace = " ";
                    }

                    effectText.Append($"{leadingSpace}{deltaText.Trim()}");
                }

                previousBuilder = builder;
            }

            return effectText.ToString().Trim();
        }

        public static bool MeetsAnyRequirements(List<ConceptualTokenEvaluatorBuilder> concepts, CampaignContext campaign, IEffectOwner owner, Combatant user, ICombatantTarget target)
        {
            TokenEvaluatorBuilder previousBuilder = null;

            foreach (ConceptualTokenEvaluatorBuilder builder in concepts)
            {
                TokenEvaluatorBuilder realizedBuilder = ScriptTokenEvaluator.RealizeConceptualBuilder(builder, campaign, owner, user, target, previousBuilder);
                if (realizedBuilder.MeetsElementRequirements(campaign.CurrentCombatContext) && realizedBuilder.MeetsRequirements(campaign.CurrentCombatContext))
                {
                    return true;
                }
                previousBuilder = realizedBuilder;
            }

            return false;
        }

        public static HashSet<StatusEffect> GetMentionedStatusEffects(IAttackTokenHolder holder)
        {
            HashSet<StatusEffect> mentionedStatusEffects = new HashSet<StatusEffect>();
            List<ConceptualTokenEvaluatorBuilder> builders = CalculateConceptualBuildersFromTokenEvaluation(holder);

            foreach (ConceptualTokenEvaluatorBuilder builder in builders)
            {
                if (builder.StatusEffect != null)
                {
                    mentionedStatusEffects.Add(builder.StatusEffect);
                }
            }

            return mentionedStatusEffects;
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