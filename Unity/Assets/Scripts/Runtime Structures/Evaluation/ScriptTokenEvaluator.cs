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

            if (evaluatedAttack.Owner is Card card)
            {
                builder.RelevantCards = new SelfCardEvaluatableValue(card);
            }

            if (evaluatedAttack.BaseElementGain != null || builder.ElementResourceChanges.Count > 0)
            {
                List<ElementResourceChange> elementChanges = new List<ElementResourceChange>();

                ConceptualTokenEvaluatorBuilder elementGainBuilder = new ConceptualTokenEvaluatorBuilder(context, builder)
                {
                    Owner = evaluatedAttack.Owner,
                    RelevantCards = builder.RelevantCards
                };

                if (evaluatedAttack.BaseElementGain != null && evaluatedAttack.BaseElementGain.Count > 0)
                {
                    foreach (Element curElement in evaluatedAttack.BaseElementGain.Keys)
                    {
                        elementGainBuilder.ElementResourceChanges.Add(new ElementResourceChange(curElement, new ConstantEvaluatableValue<int>(evaluatedAttack.BaseElementGain[curElement])));
                    }
                }

                if (builder.ElementResourceChanges != null)
                {
                    elementGainBuilder.ElementResourceChanges.AddRange(builder.ElementResourceChanges);
                }

                builders.Add(elementGainBuilder);
                builder = new ConceptualTokenEvaluatorBuilder(elementGainBuilder);
            }

            int finalIndex = evaluatedAttack.AttackTokens.Count - 1;
            for (int scriptIndex = 0; scriptIndex < evaluatedAttack.AttackTokens.Count; scriptIndex++)
            {
                IScriptingToken token = evaluatedAttack.AttackTokens[scriptIndex];

                // If this token is not a requirement, and the previous one was,
                // launch now to make sure that we're evaluating the requirements
                // in the intended context
                // i.e. "[IFTARGET: SELF][SETTARGET: FOE]" should work
                if (scriptIndex > 0 && evaluatedAttack.AttackTokens[scriptIndex-1] is IRequirement && !(token is IRequirement))
                {
                    builders.Add(builder);
                    builder = new ConceptualTokenEvaluatorBuilder(builder);
                }

                token.ApplyToken(builder);
                builder.AppliedTokens.Add(token);

                // If this is the last index in the builders, launch it now.
                // Launch it if there is any value for Intensity, as well; everything that applies intensity implies separate action
                if (builder.ShouldLaunch || scriptIndex == finalIndex)
                {
                    builders.Add(builder);
                    builder = new ConceptualTokenEvaluatorBuilder(builder);
                }
            }

            return builders;
        }

        public static GamestateDelta CalculateRealizedDeltaEvaluation(IAttackTokenHolder evaluatedAttack, CampaignContext campaignContext, Combatant user, ICombatantTarget originalTarget, ReactionWindowContext? context = null)
        {
            List<ConceptualTokenEvaluatorBuilder> concepts = CalculateConceptualBuildersFromTokenEvaluation(evaluatedAttack, context);
            return RealizeConceptualBuilders(concepts, campaignContext, evaluatedAttack.Owner, user, originalTarget);
        }

        public static GamestateDelta RealizeConceptualBuilders(List<ConceptualTokenEvaluatorBuilder> concepts, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget originalTarget)
        {
            GamestateDelta resultingDelta = new GamestateDelta();
            TokenEvaluatorBuilder previousBuilder = null;

            bool triggerRequirementsCheck = false;

            foreach (ConceptualTokenEvaluatorBuilder conceptBuilder in concepts)
            {
                if (previousBuilder != null && previousBuilder.BasedOnConcept != null && conceptBuilder.HasSameRequirements(previousBuilder.BasedOnConcept))
                {
                    // no - op; 
                }
                else
                {
                    triggerRequirementsCheck = true;
                }

                TokenEvaluatorBuilder realizedBuilder = RealizeConceptualBuilder(conceptBuilder, campaignContext, owner, user, originalTarget, previousBuilder);
                if (
                    !triggerRequirementsCheck ||
                    (realizedBuilder.MeetsElementRequirements(campaignContext.CurrentCombatContext) && realizedBuilder.MeetsRequirements(campaignContext.CurrentCombatContext))
                    )
                {
                    resultingDelta.AppendDelta(realizedBuilder.GetEffectiveDelta(campaignContext));
                    triggerRequirementsCheck = false;
                }

                previousBuilder = realizedBuilder;
            }

            return resultingDelta;
        }

        public static TokenEvaluatorBuilder RealizeConceptualBuilder(ConceptualTokenEvaluatorBuilder concept, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget originalTarget, TokenEvaluatorBuilder previousBuilder = null)
        {
            return new TokenEvaluatorBuilder(concept, campaignContext, owner, user, originalTarget, concept.ElementResourceChanges, concept.Intensity, concept.IntensityKindType, concept.RealizedOperationScriptingToken, previousBuilder);
        }

        public static GamestateDelta GetDeltaFromTokens(string attackTokens, CampaignContext context, IEffectOwner owner, Combatant user, ICombatantTarget target)
        {
            IAttackTokenHolder pile = ScriptingTokenDatabase.GetAllTokens(attackTokens, owner);
            GamestateDelta delta = CalculateRealizedDeltaEvaluation(
                    pile,
                    context,
                    user,
                    target);

            return delta;
        }

        public static string DescribeCardText(Card importingCard, ReactionWindowContext? context = null, bool ignoreFirstElementGainInText = true)
        {
            List<ConceptualTokenEvaluatorBuilder> builders = CalculateConceptualBuildersFromTokenEvaluation(importingCard, context);
            StringBuilder effectText = new StringBuilder();
            ConceptualTokenEvaluatorBuilder previousBuilder = null;
            ConceptualTokenEvaluatorBuilder previousRequirementsBuilder = null;

            foreach (ConceptualTokenEvaluatorBuilder builder in builders)
            {
                string deltaText = "";
                string requirementsText = "";
                string leadingSpace = "";

                if (!builder.HasSameRequirements(previousRequirementsBuilder))
                {
                    requirementsText = EffectDescriberDatabase.DescribeRequirement(builder);
                    effectText.Append($"{leadingSpace}\r\n{requirementsText.Trim()}");

                    previousRequirementsBuilder = builder;
                }

                if (context.HasValue && context.Value.CampaignContext != null && context.Value.CombatantEffectOwner != null && context.Value.CombatantTarget != null)
                {
                    TokenEvaluatorBuilder realizedBuilder = RealizeConceptualBuilder(builder, context.Value.CampaignContext, importingCard, context.Value.CombatantEffectOwner, context.Value.CombatantTarget);

                    string builderText = $"{EffectDescriberDatabase.DescribeRealizedEffect(realizedBuilder, ignoreFirstElementGainInText)}";
                    if (!string.IsNullOrEmpty(builderText))
                    {
                        deltaText += $"{leadingSpace}{builderText}";
                        leadingSpace = " ";
                    }
                }
                else
                {
                    ConceptualDelta conceptualDelta = builder.GetConceptualDelta();
                    if (conceptualDelta.DeltaEntries.Count > 0)
                    {
                        string builderText = $"{EffectDescriberDatabase.DescribeConceptualEffect(conceptualDelta, ignoreElementIfCard: ignoreFirstElementGainInText)}";
                        if (!string.IsNullOrEmpty(builderText))
                        {
                            deltaText += $"{leadingSpace}{builderText}";
                            leadingSpace = " ";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(deltaText))
                {
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

            // If there's still no hits, then no this can't effect target
            return false;
        }
    }
}