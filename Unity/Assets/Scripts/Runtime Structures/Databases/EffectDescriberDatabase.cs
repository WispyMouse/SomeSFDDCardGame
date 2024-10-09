namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class EffectDescriberDatabase
    {
        public enum IsPlural { DontKnow, Yes, No };

        #region Describe Entire Effects
        /// <summary>
        /// Using a <see cref="conceptualDelta"/>, describe an effect.
        /// This effect is conceptual; it hasn't happened / isn't happening.
        /// This is used to describe Cards and Effects when they aren't happening.
        /// As it is conceptual, none of the numbers have resolved yet.
        /// </summary>
        public static string DescribeConceptualEffect(ConceptualDelta conceptualDelta, string reactionWindow = "", bool ignoreElementIfCard = true)
        {
            StringBuilder entireEffectText = new StringBuilder();
            string leadingSpace = "";

            // If all element changes are gains/modifies, and we haven't yet printed out an ability text,
            // don't include that element.
            // Include it if there's any other kind of operation happening on the elements.
            bool ignoreElement = false;
            if (conceptualDelta.Owner is Card)
            {
                ignoreElement = ignoreElementIfCard;
            }

            foreach (ConceptualDeltaEntry deltaEntry in conceptualDelta.DeltaEntries)
            {
                string nextDescriptor = string.Empty;

                if (deltaEntry.ElementResourceChanges.Count > 0)
                {
                    string elementDescription = DescribeElementChange(deltaEntry, ignoreElement);
                    if (!string.IsNullOrEmpty(elementDescription))
                    {
                        nextDescriptor += leadingSpace + elementDescription;
                        leadingSpace = " ";
                    }
                }

                if (deltaEntry.MadeFromBuilder.RealizedOperationScriptingToken != null && !deltaEntry.MadeFromBuilder.RealizedOperationScriptingToken.SkipDescribingMe)
                {
                    string realizedOperation = deltaEntry.MadeFromBuilder.RealizedOperationScriptingToken.DescribeOperationAsEffect(deltaEntry, reactionWindow);
                    if (!string.IsNullOrEmpty(realizedOperation))
                    {
                        nextDescriptor += leadingSpace + realizedOperation;
                        leadingSpace = " ";
                    }
                }

                nextDescriptor += leadingSpace;

                switch (deltaEntry.IntensityKindType)
                {
                    case TokenEvaluatorBuilder.IntensityKind.Damage:
                        nextDescriptor += DescribeConceptualDamage(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.Heal:
                        nextDescriptor += DescribeConceptualHeal(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.NumberOfCards:
                        nextDescriptor += DescribeConceptualCards(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.ApplyStatusEffect:
                        nextDescriptor += DescribeConceptualApplyStatusEffect(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.RemoveStatusEffect:
                        nextDescriptor += DescribeConceptualRemoveStatusEffect(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.SetStatusEffect:
                        nextDescriptor += DescribeConceptualSetStatusEffect(deltaEntry);
                        break;
                }

                string trimmed = nextDescriptor.Trim().Trim('.');
                if (!string.IsNullOrEmpty(trimmed))
                {
                    entireEffectText.Append($"{trimmed}.");
                    leadingSpace = " ";
                }

                ignoreElement = false;
            }

            return entireEffectText.ToString().Trim();
        }

        /// <summary>
        /// Using a <see cref="attackHolder"/>, describe an effect.
        /// This effect will be realized by the other parameters. It will have all of
        /// the numbers evaluated.
        /// This effect is written as thought it hasn't happened yet.
        /// This can be used to show the value of effects, such as Enemy Intents.
        /// </summary>
        public static string DescribeRealizedEffect(IAttackTokenHolder attackHolder, CampaignContext campaignContext, IEffectOwner owner, Combatant user, ICombatantTarget target)
        {
            StringBuilder entireEffectText = new StringBuilder();
            string leadingSpace = "";

            List<ConceptualTokenEvaluatorBuilder> conceptBuilders = ScriptTokenEvaluator.CalculateConceptualBuildersFromTokenEvaluation(attackHolder);
            foreach (ConceptualTokenEvaluatorBuilder conceptBuilder in conceptBuilders)
            {
                TokenEvaluatorBuilder realizedBuilder = ScriptTokenEvaluator.RealizeConceptualBuilder(conceptBuilder, campaignContext, owner, user, target);
                string descriptor = DescribeRealizedEffect(realizedBuilder);

                if (!string.IsNullOrEmpty(descriptor))
                {
                    entireEffectText.Append($"{leadingSpace}{descriptor}");
                    leadingSpace = " ";
                }
            }

            return entireEffectText.ToString();
        }

        /// <summary>
        /// Using a <see cref="TokenEvaluatorBuilder"/>, describe an effect.
        /// This effect will be realized by the other parameters. It will have all of
        /// the numbers evaluated.
        /// This effect is written as thought it hasn't happened yet.
        /// This can be used to show the value of effects, such as Enemy Intents.
        /// </summary>
        public static string DescribeRealizedEffect(TokenEvaluatorBuilder builder)
        {
            StringBuilder entireEffectText = new StringBuilder();

            string nextDescriptor = string.Empty;

            switch (builder.IntensityKindType)
            {
                case TokenEvaluatorBuilder.IntensityKind.Damage:
                    nextDescriptor = DescribeRealizedDamage(builder);
                    break;
                case TokenEvaluatorBuilder.IntensityKind.Heal:
                    nextDescriptor = DescribeRealizedHeal(builder);
                    break;
                case TokenEvaluatorBuilder.IntensityKind.NumberOfCards:
                    nextDescriptor = DescribeRealizedCards(builder);
                    break;
                case TokenEvaluatorBuilder.IntensityKind.ApplyStatusEffect:
                    nextDescriptor = DescribeRealizedApplyStatusEffect(builder);
                    break;
                case TokenEvaluatorBuilder.IntensityKind.RemoveStatusEffect:
                    nextDescriptor = DescribeRealizedRemoveStatusEffect(builder);
                    break;
                case TokenEvaluatorBuilder.IntensityKind.SetStatusEffect:
                    nextDescriptor = DescribeRealizedSetStatusEffect(builder);
                    break;
            }

            if (!string.IsNullOrEmpty(nextDescriptor))
            {
                entireEffectText.Append($"{nextDescriptor}");
            }

            return entireEffectText.ToString();
        }

        /// <summary>
        /// Using a <see cref="GamestateDelta"/>, describe an effect.
        /// This effect is written as though it happens directly from this delta being applied.
        /// This is useful for log text, describing an actual change.
        /// </summary>
        public static string DescribeResolvedEffect(GamestateDelta delta)
        {
            StringBuilder entireEffectText = new StringBuilder();
            string leadingSpace = "";

            foreach (DeltaEntry deltaEntry in delta.DeltaEntries)
            {
                string nextDescriptor = string.Empty;

                switch (deltaEntry.IntensityKindType)
                {
                    case TokenEvaluatorBuilder.IntensityKind.Damage:
                        nextDescriptor = DescribeResolvedDamage(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.Heal:
                        nextDescriptor = DescribeResolvedHeal(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.NumberOfCards:
                        nextDescriptor = DescribeResolvedCards(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.ApplyStatusEffect:
                        nextDescriptor = DescribeResolvedApplyStatusEffect(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.RemoveStatusEffect:
                        nextDescriptor = DescribeResolvedRemoveStatusEffect(deltaEntry);
                        break;
                }

                if (!string.IsNullOrEmpty(nextDescriptor))
                {
                    entireEffectText.Append($"{leadingSpace}{nextDescriptor}");
                    leadingSpace = " ";
                }
            }

            return entireEffectText.ToString().Trim();
        }

        public static string DescribeDrainEffect(string argumentOne, string argumentTwo)
        {
            return $"{argumentOne} will apply to {argumentTwo} before Health.";
        }
        #endregion

        #region Describing Damage
        public static string DescribeConceptualDamage(ConceptualDeltaEntry deltaEntry)
        {
            return ComposeDescriptor(
                $"to {deltaEntry.ConceptualTarget.DescribeEvaluation().ToLower()}",
                deltaEntry.ConceptualTarget,
                deltaEntry.OriginalConceptualTarget,
                deltaEntry.PreviousConceptualTarget,
                deltaEntry.ConceptualIntensity,
                String.Empty,
                String.Empty,
                "damage",
                ComposeValueTargetLocation.AfterSuffix,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                (CombatantTargetEvaluatableValue curValue) =>
                {
                    return curValue is FoeTargetEvaluatableValue;
                });
        }

        public static string DescribeRealizedDamage(TokenEvaluatorBuilder builder)
        {
            return ComposeDescriptor<ICombatantTarget>(
                $"to {builder.Target.Name}",
                builder.Target,
                builder.OriginalTarget,
                builder?.PreviousTokenBuilder?.Target,
                builder.Intensity.DescribeEvaluation(builder.Campaign, builder),
                String.Empty,
                String.Empty,
                "damage",
                ComposeValueTargetLocation.AfterSuffix,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                (ICombatantTarget curValue) =>
                {
                    return (builder.Target == builder.OriginalTarget && builder.Target == builder?.PreviousTokenBuilder?.Target
                    && builder.Target.IsFoeOf(builder.User)
                    && builder.Target.GetRepresentingNumberOfTargets() == 1);
                });
        }

        public static string DescribeResolvedDamage(DeltaEntry delta)
        {
            if (!delta.ConceptualIntensity.TryEvaluateValue(delta.FromCampaign, delta.MadeFromBuilder, out int evaluatedValue))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse conceptual intensity, after it already should be resolveable.", GlobalUpdateUX.LogType.RuntimeError);
                return String.Empty;
            }

            return ComposeDescriptor<ICombatantTarget>(
                $"to {delta.Target.Name}",
                delta.Target,
                delta.OriginalTarget,
                delta.MadeFromBuilder?.PreviousTokenBuilder?.Target,
                evaluatedValue.ToString(),
                String.Empty,
                String.Empty,
                "damage",
                ComposeValueTargetLocation.AfterSuffix,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                null);
        }
        #endregion

        #region Describing Heal
        public static string DescribeConceptualHeal(ConceptualDeltaEntry deltaEntry)
        {
            return ComposeDescriptor(
                $"{deltaEntry.ConceptualTarget.DescribeEvaluation().ToLower()} for",
                deltaEntry.ConceptualTarget,
                deltaEntry.OriginalConceptualTarget,
                deltaEntry.PreviousConceptualTarget,
                deltaEntry.ConceptualIntensity,
                "Heal",
                String.Empty,
                String.Empty,
                ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                (CombatantTargetEvaluatableValue curValue) => 
                {
                    return curValue is SelfTargetEvaluatableValue
                        && (deltaEntry.PreviousConceptualTarget == null || deltaEntry.PreviousConceptualTarget == deltaEntry.ConceptualTarget);
                }
                );
        }

        public static string DescribeRealizedHeal(TokenEvaluatorBuilder builder)
        {
            return ComposeDescriptor<ICombatantTarget>(
                $"{builder.Target.Name} for",
                builder.Target,
                builder.OriginalTarget,
                builder?.PreviousTokenBuilder?.Target,
                builder.Intensity.DescribeEvaluation(builder.Campaign, builder),
                "Heal",
                String.Empty,
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                (ICombatantTarget curValue) =>
                {
                    return (builder.Target == builder.User);
                });
        }

        public static string DescribeResolvedHeal(DeltaEntry delta)
        {
            return ComposeDescriptor<ICombatantTarget>(
                $"{delta.Target.Name} for",
                delta.Target,
                delta.OriginalTarget,
                delta.MadeFromBuilder?.PreviousTokenBuilder?.Target,
                delta.ConceptualIntensity.ToString(),
                "Heal",
                String.Empty,
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                null);
        }
        #endregion

        #region Describing Cards
        public static string DescribeConceptualCards(ConceptualDeltaEntry deltaEntry)
        {
            string valueText = deltaEntry.ConceptualIntensity.DescribeEvaluation();

            switch (deltaEntry.IntensityKindType)
            {
                case TokenEvaluatorBuilder.IntensityKind.NumberOfCards:
                    if (deltaEntry.NumberOfCardsRelationType == TokenEvaluatorBuilder.NumberOfCardsRelation.Draw)
                    {
                        if (deltaEntry.ConceptualIntensity is ConstantEvaluatableValue<int> constant && constant.ConstantValue == 1)
                        {
                            valueText = "a";
                        }

                        return ComposeDescriptor<CombatantTargetEvaluatableValue>(
                            String.Empty,
                            null,
                            null,
                            null,
                            valueText,
                            "Draw ",
                            ExtractSingularOrPlural(deltaEntry.ConceptualIntensity, "card"),
                            String.Empty,
                            ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                            ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                            null
                            );
                    }
                    break;
            }

            return String.Empty;
        }

        public static string DescribeRealizedCards(TokenEvaluatorBuilder builder)
        {
            string valueText = builder.Intensity.DescribeEvaluation(builder.Campaign, builder);

            switch (builder.IntensityKindType)
            {
                case TokenEvaluatorBuilder.IntensityKind.NumberOfCards:
                    if (builder.NumberOfCardsRelationType == TokenEvaluatorBuilder.NumberOfCardsRelation.Draw)
                    {
                        if (builder.Intensity is ConstantEvaluatableValue<int> constant && constant.ConstantValue == 1)
                        {
                            valueText = "a";
                        }

                        return ComposeDescriptor<ICombatantTarget>(
                            String.Empty,
                            null,
                            null,
                            null,
                            valueText,
                            "Draw ",
                            ExtractSingularOrPlural(builder.Intensity, "card"),
                            String.Empty,
                            ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                            ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                            null
                            );
                    }
                    break;
            }

            return String.Empty;
        }

        public static string DescribeResolvedCards(DeltaEntry delta)
        {
            switch (delta.IntensityKindType)
            {
                case TokenEvaluatorBuilder.IntensityKind.NumberOfCards:
                    if (delta.NumberOfCardsRelationType == TokenEvaluatorBuilder.NumberOfCardsRelation.Draw)
                    {
                        return ComposeDescriptor(
                            String.Empty,
                            null,
                            null,
                            null,
                            delta.ConceptualIntensity,
                            "Draw ",
                            ExtractSingularOrPlural(delta.ConceptualIntensity, "card"),
                            String.Empty,
                            ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                            ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                            null
                            );
                    }
                    break;
            }

            return String.Empty;
        }
        #endregion

        #region Describing Status Effect
        public static string DescribeConceptualApplyStatusEffect(ConceptualDeltaEntry deltaEntry)
        {
            if (deltaEntry.ConceptualTarget is SelfTargetEvaluatableValue)
            {
                return $"Gain {deltaEntry.ConceptualIntensity.DescribeEvaluation()} {deltaEntry.StatusEffect.Name}.";
            }

            // If this stack change is targeting the Owner of this entire effect,
            // and it's being applied to the target that holds this status,
            // don't mention the name of the stacks
            string maybeName = $"{deltaEntry.StatusEffect.Name}";
            string toTargetText = $"to {deltaEntry.ConceptualTarget.DescribeEvaluation().ToLower()}";
            if (deltaEntry.MadeFromBuilder.Owner is StatusEffect ownerStatus 
                && deltaEntry.StatusEffect != null 
                && deltaEntry.StatusEffect.Equals(ownerStatus)
                && deltaEntry.ConceptualTarget is SelfTargetEvaluatableValue)
            {
                maybeName = $"";
                toTargetText = string.Empty;
            }

            return ComposeDescriptor(
                toTargetText,
                deltaEntry.ConceptualTarget,
                deltaEntry.OriginalConceptualTarget,
                deltaEntry.PreviousConceptualTarget,
                deltaEntry.ConceptualIntensity,
                "Apply",
                maybeName,
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                (CombatantTargetEvaluatableValue curValue) =>
                {
                    return deltaEntry.PreviousConceptualTarget != null && deltaEntry.ConceptualTarget != null && deltaEntry.OriginalConceptualTarget != null 
                    && deltaEntry.PreviousConceptualTarget.Equals(deltaEntry.ConceptualTarget) && deltaEntry.ConceptualTarget.Equals(deltaEntry.OriginalConceptualTarget);
                });
        }

        public static string DescribeRealizedApplyStatusEffect(TokenEvaluatorBuilder builder)
        {
            if (builder.Target == builder.User)
            {
                return $"Gain {builder.Intensity.DescribeEvaluation(builder.Campaign, builder)} {builder.StatusEffect.Name}.";
            }

            string stackstext = ExtractSingularOrPlural(builder.Intensity, "stack");

            // If this stack change is targeting the Owner of this entire effect,
            // and it's being applied to the target that holds this status,
            // don't mention the name of the stacks
            string maybeName = $"{builder.StatusEffect.Name}";
            string toTargetText = $"to {builder.Target.Name}";
            if (builder.Owner is StatusEffect ownerStatus
                && builder.StatusEffect != null
                && builder.StatusEffect.Equals(ownerStatus)
                && builder.Target == builder.User)
            {
                maybeName = $"";
                toTargetText = string.Empty;
            }

            return ComposeDescriptor<ICombatantTarget>(
                toTargetText,
                builder.Target,
                builder.OriginalTarget,
                builder?.PreviousTokenBuilder?.Target,
                builder.Intensity.ToString(),
                "Apply",
                maybeName,
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                (ICombatantTarget curValue) =>
                {
                    return (builder.Target == builder.OriginalTarget && builder.Target == builder?.PreviousTokenBuilder?.Target
                    && builder.Target.IsFoeOf(builder.User)
                    && builder.Target.GetRepresentingNumberOfTargets() == 1);
                });
        }

        public static string DescribeResolvedApplyStatusEffect(DeltaEntry delta)
        {
            string stackstext = ExtractSingularOrPlural(delta.ConceptualIntensity, "stack");

            return ComposeDescriptor<ICombatantTarget>(
                $"to {delta.Target.Name}",
                delta.Target,
                delta.OriginalTarget,
                delta.MadeFromBuilder?.PreviousTokenBuilder?.Target,
                delta.ConceptualIntensity.ToString(),
                "Apply",
                $"{stackstext} of {delta.StatusEffect.Name}",
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                null);
        }

        public static string DescribeConceptualRemoveStatusEffect(ConceptualDeltaEntry deltaEntry)
        {
            string stackstext = ExtractSingularOrPlural(deltaEntry.ConceptualIntensity, "stack");

            // If this stack change is targeting the Owner of this entire effect,
            // and it's being applied to the target that holds this status,
            // don't mention the name of the stacks
            string stacksTextWithMaybeName = $"{stackstext} of {deltaEntry.StatusEffect.Name}";
            string toTargetText = $"from {deltaEntry.ConceptualTarget.DescribeEvaluation().ToLower()}";
            if (deltaEntry.MadeFromBuilder.Owner is StatusEffect ownerStatus
                && deltaEntry.StatusEffect != null
                && deltaEntry.StatusEffect.Equals(ownerStatus)
                && deltaEntry.ConceptualTarget is SelfTargetEvaluatableValue)
            {
                stacksTextWithMaybeName = $"{stackstext}";
                toTargetText = string.Empty;
            }

            return ComposeDescriptor(
                toTargetText,
                deltaEntry.ConceptualTarget,
                deltaEntry.OriginalConceptualTarget,
                deltaEntry.PreviousConceptualTarget,
                deltaEntry.ConceptualIntensity,
                "Remove",
                stacksTextWithMaybeName,
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                null
                );
        }

        public static string DescribeRealizedRemoveStatusEffect(TokenEvaluatorBuilder builder)
        {
            string stackstext = ExtractSingularOrPlural(builder.Intensity, "stack");

            // If this stack change is targeting the Owner of this entire effect,
            // and it's being applied to the target that holds this status,
            // don't mention the name of the stacks
            string stacksTextWithMaybeName = $"{stackstext} of {builder.StatusEffect.Name}";
            string toTargetText = $"from {builder.Target.Name}";
            if (builder.Owner is StatusEffect ownerStatus
                && builder.StatusEffect != null
                && builder.StatusEffect.Equals(ownerStatus)
                && builder.Target == builder.User)
            {
                stacksTextWithMaybeName = $"{stackstext}";
                toTargetText = string.Empty;
            }

            return ComposeDescriptor<ICombatantTarget>(
                toTargetText,
                builder.Target,
                builder.OriginalTarget,
                builder?.PreviousTokenBuilder?.Target,
                builder.Intensity.DescribeEvaluation(builder.Campaign, builder),
                "Remove",
                stacksTextWithMaybeName,
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                (ICombatantTarget curValue) =>
                {
                    return (builder.Target == builder.OriginalTarget && builder.Target == builder?.PreviousTokenBuilder?.Target
                    && builder.Target.IsFoeOf(builder.User)
                    && builder.Target.GetRepresentingNumberOfTargets() == 1);
                });
        }

        public static string DescribeResolvedRemoveStatusEffect(DeltaEntry delta)
        {
            string stackstext = ExtractSingularOrPlural(delta.ConceptualIntensity, "stack");

            return ComposeDescriptor<ICombatantTarget>(
                $"from {delta.Target.Name}",
                delta.Target,
                delta.OriginalTarget,
                delta.MadeFromBuilder?.PreviousTokenBuilder?.Target,
                delta.ConceptualIntensity.ToString(),
                "Remove",
                $"{stackstext} of {delta.StatusEffect.Name}",
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                null);
        }

        public static string DescribeConceptualSetStatusEffect(ConceptualDeltaEntry deltaEntry)
        {
            if (deltaEntry.ConceptualTarget is SelfTargetEvaluatableValue && deltaEntry.ConceptualIntensity is ConstantEvaluatableValue<int> constant && constant.ConstantValue == 0)
            {
                return $"Clear {deltaEntry.StatusEffect.Name}.";
            }

            string stackstext = ExtractSingularOrPlural(deltaEntry.ConceptualIntensity, "stack");

            // If this stack change is targeting the Owner of this entire effect,
            // and it's being applied to the target that holds this status,
            // don't mention the name of the stacks
            string stacksTextWithMaybeName = $"Set {deltaEntry.StatusEffect.Name} to";
            string toTargetText = $"on {deltaEntry.ConceptualTarget.DescribeEvaluation().ToLower()}";
            if (deltaEntry.MadeFromBuilder.Owner is StatusEffect ownerStatus
                && deltaEntry.StatusEffect != null
                && deltaEntry.StatusEffect.Equals(ownerStatus)
                && deltaEntry.ConceptualTarget is SelfTargetEvaluatableValue)
            {
                toTargetText = string.Empty;
            }

            return ComposeDescriptor(
                toTargetText,
                deltaEntry.ConceptualTarget,
                deltaEntry.OriginalConceptualTarget,
                deltaEntry.PreviousConceptualTarget,
                deltaEntry.ConceptualIntensity,
                stacksTextWithMaybeName,
                $"{stackstext}",
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                null
                );
        }

        public static string DescribeRealizedSetStatusEffect(TokenEvaluatorBuilder builder)
        {
            if (builder.Intensity is ConstantEvaluatableValue<int> constant && constant.ConstantValue == 0)
            {
                return $"Clear {builder.StatusEffect.Name}.";
            }

            string stackstext = ExtractSingularOrPlural(builder.Intensity, "stack");

            // If this stack change is targeting the Owner of this entire effect,
            // and it's being applied to the target that holds this status,
            // don't mention the name of the stacks
            string stacksTextWithMaybeName = $"Set {builder.StatusEffect.Name} to";
            string toTargetText = $"on {builder.Target.Name}";
            if (builder.Owner is StatusEffect ownerStatus
                && builder.StatusEffect != null
                && builder.StatusEffect.Equals(ownerStatus)
                && builder.Target.Equals(builder.Owner))
            {
                toTargetText = string.Empty;
            }

            return ComposeDescriptor<ICombatantTarget>(
                toTargetText,
                builder.Target,
                builder.OriginalTarget,
                builder?.PreviousTokenBuilder?.Target,
                builder.Intensity.DescribeEvaluation(builder.Campaign, builder),
                stacksTextWithMaybeName,
                $"{stackstext}",
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenPrefixAndMiddle,
                (ICombatantTarget curValue) =>
                {
                    return (builder.Target == builder.OriginalTarget && builder.Target == builder?.PreviousTokenBuilder?.Target
                    && builder.Target.IsFoeOf(builder.User)
                    && builder.Target.GetRepresentingNumberOfTargets() == 1);
                });
        }

        public static string ExtractSingularOrPlural(IEvaluatableValue<int> evaluatable, string root)
        {
            return ExtractSingularOrPlural(evaluatable, root, $"{root}s", $"{root}(s)");
        }

        public static string ExtractSingularOrPlural(int toUtilize, string root)
        {
            return ExtractSingularOrPlural(toUtilize, root, $"{root}s", $"{root}(s)");
        }

        public static string ExtractSingularOrPlural(IEvaluatableValue<int> evaluatable, string single, string plural, string dontknow)
        {
            switch (DeterminePlurality(evaluatable))
            {
                case IsPlural.Yes:
                    return plural;
                case IsPlural.No:
                    return single;
                default:
                case IsPlural.DontKnow:
                    return dontknow;
            }
        }

        public static string ExtractSingularOrPlural(int toUse, string single, string plural, string dontknow)
        {
            switch (DeterminePlurality(toUse))
            {
                case IsPlural.Yes:
                    return plural;
                case IsPlural.No:
                    return single;
                default:
                case IsPlural.DontKnow:
                    return dontknow;
            }
        }

        public static IsPlural DeterminePlurality(int toDetermine)
        {
            if (toDetermine == 1)
            {
                return IsPlural.No;
            }
            else
            {
                return IsPlural.Yes;
            }
        }

        public static IsPlural DeterminePlurality(IEvaluatableValue<int> evaluatable)
        {
            if (evaluatable is ConstantEvaluatableValue<int> constant)
            {
                return DeterminePlurality(constant.ConstantValue);
            }

            return IsPlural.DontKnow;
        }
        #endregion

        #region General Descriptor Handlers
        public enum ComposeValueTargetLocation
        {
            BeforePrefix,
            BetweenPrefixAndMiddle,
            BetweenMiddleAndSuffix,
            AfterSuffix
        }

        public delegate bool ShouldRemoveTarget<T>(T currentTarget);

        public static string ComposeDescriptor
            (
            string targetDescriptor,
            CombatantTargetEvaluatableValue target,
            CombatantTargetEvaluatableValue originalTarget,
            CombatantTargetEvaluatableValue previousTarget,
            IEvaluatableValue<int> value,
            string prefix,
            string middle,
            string suffix,
            ComposeValueTargetLocation whereToPutTarget,
            ComposeValueTargetLocation whereToPutValue,
            ShouldRemoveTarget<CombatantTargetEvaluatableValue> omitTargetDelegate)
        {
            string valueDescription = value.DescribeEvaluation();

            return ComposeDescriptor<CombatantTargetEvaluatableValue>
                (
                    targetDescriptor,
                    target,
                    originalTarget,
                    previousTarget,
                    valueDescription,
                    prefix,
                    middle,
                    suffix,
                    whereToPutTarget,
                    whereToPutValue,
                    omitTargetDelegate
                );
        }

        public static string ComposeDescriptor(
            string targetDescriptor,
            ICombatantTarget target,
            ICombatantTarget originalTarget,
            ICombatantTarget previousTarget,
            int value, 
            string prefix,
            string middle,
            string suffix,
            ComposeValueTargetLocation whereToPutTarget,
            ComposeValueTargetLocation whereToPutValue,
            ShouldRemoveTarget<ICombatantTarget> omitTargetDelegate)
        {
            return ComposeDescriptor(
                targetDescriptor,
                target, 
                originalTarget,
                previousTarget,
                value.ToString(),
                prefix, 
                middle,
                suffix, 
                whereToPutTarget, 
                whereToPutValue,
                omitTargetDelegate);
        }

        public static string ComposeDescriptor<T>(
            string targetText, 
            T target, 
            T originalTarget,
            T previoustarget,
            string valueText, 
            string prefix, 
            string middle,
            string suffix,
            ComposeValueTargetLocation whereToPutTarget,
            ComposeValueTargetLocation whereToPutValue,
            ShouldRemoveTarget<T> omitTargetDelegate) where T : IEquatable<T>
        {
            StringBuilder descriptor = new StringBuilder();
            string leadingSpace = "";

            bool shouldPutTarget = true;

            if (omitTargetDelegate != null)
            {
                shouldPutTarget &= !omitTargetDelegate(target);
            }

            if (!string.IsNullOrEmpty(valueText) && whereToPutValue == ComposeValueTargetLocation.BeforePrefix)
            {
                descriptor.Append($"{leadingSpace}{valueText.Trim()}");
                leadingSpace = " ";
            }

            if (!string.IsNullOrEmpty(targetText) && shouldPutTarget && whereToPutTarget == ComposeValueTargetLocation.BeforePrefix)
            {
                descriptor.Append($"{leadingSpace}{targetText.Trim()}");
                leadingSpace = " ";
            }

            if (!string.IsNullOrEmpty(prefix))
            {
                descriptor.Append($"{leadingSpace}{prefix.Trim()}");
                leadingSpace = " ";
            }

            if (!string.IsNullOrEmpty(valueText) && whereToPutValue == ComposeValueTargetLocation.BetweenPrefixAndMiddle)
            {
                descriptor.Append($"{leadingSpace}{valueText.Trim()}");
                leadingSpace = " ";
            }

            if (!string.IsNullOrEmpty(targetText) && shouldPutTarget && whereToPutTarget == ComposeValueTargetLocation.BetweenPrefixAndMiddle)
            {
                descriptor.Append($"{leadingSpace}{targetText.Trim()}");
                leadingSpace = " ";
            }

            if (!string.IsNullOrEmpty(middle))
            {
                descriptor.Append($"{leadingSpace}{middle.Trim()}");
                leadingSpace = " ";
            }

            if (!string.IsNullOrEmpty(valueText) && whereToPutValue == ComposeValueTargetLocation.BetweenMiddleAndSuffix)
            {
                descriptor.Append($"{leadingSpace}{valueText.Trim()}");
                leadingSpace = " ";
            }

            if (!string.IsNullOrEmpty(targetText) && shouldPutTarget && whereToPutTarget == ComposeValueTargetLocation.BetweenMiddleAndSuffix)
            {
                descriptor.Append($"{leadingSpace}{targetText.Trim()}");
                leadingSpace = " ";
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                descriptor.Append($"{leadingSpace}{suffix.Trim()}");
                leadingSpace = " ";
            }

            if (!string.IsNullOrEmpty(valueText) && whereToPutValue == ComposeValueTargetLocation.AfterSuffix)
            {
                descriptor.Append($"{leadingSpace}{valueText.Trim()}");
                leadingSpace = " ";
            }

            if (!string.IsNullOrEmpty(targetText) && shouldPutTarget && whereToPutTarget == ComposeValueTargetLocation.AfterSuffix)
            {
                descriptor.Append($"{leadingSpace}{targetText.Trim()}");
                leadingSpace = " ";
            }

            string builtString = descriptor.ToString().Trim();

            if (!string.IsNullOrEmpty(builtString) && !builtString.EndsWith("."))
            {
                builtString += ".";
            }

            return builtString.Trim();
        }
        #endregion

        #region Describing Elements

        public static string DescribeElementChange(ConceptualDeltaEntry delta, bool ignoreElementFlag)
        {
            StringBuilder changeText = new StringBuilder();
            string leadingcomma = "";

            foreach (ElementResourceChange change in delta.ElementResourceChanges)
            {
                StringBuilder nextText = new StringBuilder();

                if (change.SetValue != null)
                {
                    if (change.SetValue is ConstantEvaluatableValue<int> constant && constant.ConstantValue == 0)
                    {
                        nextText.Append($"{leadingcomma}Clear {change.Element.GetNameAndMaybeIcon()}");
                    }
                    else
                    {
                        nextText.Append($"{leadingcomma}Set {change.Element.GetNameAndMaybeIcon()} to {change.SetValue.DescribeEvaluation()}");
                    }

                    leadingcomma = ", ";
                }
                else if (change.GainOrLoss != null)
                {
                    if (change.GainOrLoss is ConstantEvaluatableValue<int> constant)
                    {
                        if (constant.ConstantValue > 0 && !ignoreElementFlag)
                        {
                            nextText.Append($"{leadingcomma}Gain {constant.ConstantValue.ToString()} {change.Element.GetNameAndMaybeIcon()}");
                            leadingcomma = ", ";
                        }
                        else if (constant.ConstantValue < 0)
                        {
                            nextText.Append($"{leadingcomma}Lose {Mathf.Abs(constant.ConstantValue).ToString()} {change.Element.GetNameAndMaybeIcon()}");
                            leadingcomma = ", ";
                        }
                    }
                    else
                    {
                        nextText.Append($"{leadingcomma}Modify {change.Element.GetNameAndMaybeIcon()} by {change.GainOrLoss.DescribeEvaluation()}");
                        leadingcomma = ", ";
                    }
                }

                string builderResult = nextText.ToString().Trim('.');

                // If this had any text, it should always end in a period
                if (builderResult.Length > 0)
                {
                    builderResult += ".";
                    changeText.Append(builderResult);
                }
            }

            return changeText.ToString();
        }

        public static string DescribeRequirement(ConceptualTokenEvaluatorBuilder delta)
        {
            StringBuilder resultBuilder = new StringBuilder();

            string leadingComma = "";
            string startingLine = "If ";

            if (delta.ElementRequirements.Count > 0)
            {
                foreach (Element req in delta.ElementRequirements.Keys)
                {
                    string thisRequirementText = delta.ElementRequirements[req].DescribeEvaluation();
                    if (!string.IsNullOrEmpty(thisRequirementText))
                    {
                        resultBuilder.Append($"{startingLine}{leadingComma}{req.GetNameAndMaybeIcon()} >= {thisRequirementText}");
                        leadingComma = ", ";
                        startingLine = "";
                    }
                }
            }

            foreach (IRequirement requirement in delta.Requirements)
            {
                string thisRequirementText = requirement.DescribeRequirement();
                if (!string.IsNullOrEmpty(thisRequirementText))
                {
                    if (thisRequirementText.StartsWith("1 x "))
                    {
                        thisRequirementText = thisRequirementText.Substring("1 x ".Length);
                    }

                    resultBuilder.Append($"{startingLine}{leadingComma}{thisRequirementText}");
                    leadingComma = ", ";
                    startingLine = "";
                }
            }

            if (resultBuilder.Length > 0)
            {
                resultBuilder.Append(":");
            }

            return resultBuilder.ToString();
        }

        #endregion
    }
}