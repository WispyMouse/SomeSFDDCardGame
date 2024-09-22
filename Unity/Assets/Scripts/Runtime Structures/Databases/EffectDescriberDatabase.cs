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
        #region Describe Entier Effects
        /// <summary>
        /// Using a <see cref="conceptualDelta"/>, describe an effect.
        /// This effect is conceptual; it hasn't happened / isn't happening.
        /// This is used to describe Cards and Effects when they aren't happening.
        /// As it is conceptual, none of the numbers have resolved yet.
        /// </summary>
        public static string DescribeConceptualEffect(ConceptualDelta conceptualDelta)
        {
            StringBuilder entireEffectText = new StringBuilder();
            string leadingSpace = "";

            foreach (ConceptualDeltaEntry deltaEntry in conceptualDelta.DeltaEntries)
            {
                string nextDescriptor = string.Empty;

                switch (deltaEntry.IntensityKindType)
                {
                    case TokenEvaluatorBuilder.IntensityKind.Damage:
                        nextDescriptor = DescribeConceptualDamage(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.Heal:
                        nextDescriptor = DescribeConceptualHeal(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.NumberOfCards:
                        nextDescriptor = DescribeConceptualDamage(deltaEntry);
                        break;
                    case TokenEvaluatorBuilder.IntensityKind.StatusEffect:
                        nextDescriptor = DescribeConceptualStatusEffect(deltaEntry);
                        break;
                }

                if (!string.IsNullOrEmpty(nextDescriptor))
                {
                    entireEffectText.Append($"{leadingSpace}{nextDescriptor}");
                    leadingSpace = " ";
                }
            }

            return entireEffectText.ToString();
        }

        /// <summary>
        /// Using a <see cref="attackHolder"/>, describe an effect.
        /// This effect will be realized by the other parameters. It will have all of
        /// the numbers evaluated.
        /// This effect is written as thought it hasn't happened yet.
        /// This can be used to show the value of effects, such as Enemy Intents.
        /// </summary>
        public static string DescribeRealizedEffect(IAttackTokenHolder attackHolder, CampaignContext campaignContext, ICombatantTarget user, ICombatantTarget target)
        {
            StringBuilder entireEffectText = new StringBuilder();
            string leadingSpace = "";

            List<ConceptualTokenEvaluatorBuilder> conceptBuilders = ScriptTokenEvaluator.CalculateConceptualBuildersFromTokenEvaluation(attackHolder);
            foreach (ConceptualTokenEvaluatorBuilder conceptBuilder in conceptBuilders)
            {
                TokenEvaluatorBuilder realizedBuilder = ScriptTokenEvaluator.RealizeConceptualBuilder(conceptBuilder, campaignContext, user, target);
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
                case TokenEvaluatorBuilder.IntensityKind.StatusEffect:
                    nextDescriptor = DescribeRealizedStatusEffect(builder);
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
                    case TokenEvaluatorBuilder.IntensityKind.StatusEffect:
                        nextDescriptor = DescribeResolvedStatusEffect(deltaEntry);
                        break;
                }

                if (!string.IsNullOrEmpty(nextDescriptor))
                {
                    entireEffectText.Append($"{leadingSpace}{nextDescriptor}");
                    leadingSpace = " ";
                }
            }

            return entireEffectText.ToString();
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
                    return !(curValue is FoeTargetEvaluatableValue);
                });
        }

        public static string DescribeRealizedDamage(TokenEvaluatorBuilder builder)
        {
            return ComposeDescriptor<ICombatantTarget>(
                $"to {builder.Target.Name}",
                builder.Target,
                builder.OriginalTarget,
                builder?.PreviousTokenBuilder?.Target,
                builder.Intensity.ToString(),
                String.Empty,
                "damage",
                builder.GetIntensityDescriptionIfNotConstant(),
                ComposeValueTargetLocation.AfterSuffix,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                (ICombatantTarget curValue) =>
                {
                    return !(builder.Target == builder.OriginalTarget && builder.Target == builder?.PreviousTokenBuilder?.Target
                    && builder.Target.IsFoeOf(builder.User)
                    && builder.Target.GetRepresentingNumberOfTargets() == 1);
                });
        }

        public static string DescribeResolvedDamage(DeltaEntry delta)
        {
            return ComposeDescriptor<ICombatantTarget>(
                $"to {delta.Target.Name}",
                delta.Target,
                delta.OriginalTarget,
                delta.MadeFromBuilder?.PreviousTokenBuilder?.Target,
                delta.Intensity.ToString(),
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
                    return !(curValue is SelfTargetEvaluatableValue);
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
                builder.Intensity.ToString(),
                "Heal",
                String.Empty,
                builder.GetIntensityDescriptionIfNotConstant(),
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                (ICombatantTarget curValue) =>
                {
                    return !(builder.Target == builder.User);
                });
        }

        public static string DescribeResolvedHeal(DeltaEntry delta)
        {
            return ComposeDescriptor<ICombatantTarget>(
                $"{delta.Target.Name} for",
                delta.Target,
                delta.OriginalTarget,
                delta.MadeFromBuilder?.PreviousTokenBuilder?.Target,
                delta.Intensity.ToString(),
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
            switch (deltaEntry.IntensityKindType)
            {
                case TokenEvaluatorBuilder.IntensityKind.NumberOfCards:
                    if (deltaEntry.NumberOfCardsRelationType == TokenEvaluatorBuilder.NumberOfCardsRelation.Draw)
                    {
                        return ComposeDescriptor(
                            String.Empty,
                            null,
                            null,
                            null,
                            deltaEntry.ConceptualIntensity,
                            "Draw ",
                            "card(s)",
                            String.Empty,
                            ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                            ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                            null
                            );
                    }
                    break;
            }

            return String.Empty;
        }

        public static string DescribeRealizedCards(TokenEvaluatorBuilder builder)
        {
            switch (builder.IntensityKindType)
            {
                case TokenEvaluatorBuilder.IntensityKind.NumberOfCards:
                    if (builder.NumberOfCardsRelationType == TokenEvaluatorBuilder.NumberOfCardsRelation.Draw)
                    {
                        return ComposeDescriptor(
                            String.Empty,
                            null,
                            null,
                            null,
                            builder.Intensity,
                            "Draw ",
                            "card(s)",
                            builder.GetIntensityDescriptionIfNotConstant(),
                            ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                            ComposeValueTargetLocation.BetweenMiddleAndSuffix,
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
                            delta.Intensity,
                            "Draw ",
                            "card(s)",
                            String.Empty,
                            ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                            ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                            null
                            );
                    }
                    break;
            }

            return String.Empty;
        }
        #endregion

        #region Describing Status Effect
        public static string DescribeConceptualStatusEffect(ConceptualDeltaEntry deltaEntry)
        {
            return ComposeDescriptor(
                $"on {deltaEntry.ConceptualTarget.DescribeEvaluation().ToLower()}",
                deltaEntry.ConceptualTarget,
                deltaEntry.OriginalConceptualTarget,
                deltaEntry.PreviousConceptualTarget,
                deltaEntry.ConceptualIntensity,
                "Apply/Remove",
                $"stack(s) of {deltaEntry.StatusEffect.Name}",
                String.Empty,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                (CombatantTargetEvaluatableValue curValue) =>
                {
                    return !(curValue is SelfTargetEvaluatableValue);
                }
                ) ;
        }

        public static string DescribeRealizedStatusEffect(TokenEvaluatorBuilder builder)
        {
            return ComposeDescriptor<ICombatantTarget>(
                $"on {builder.Target.Name}",
                builder.Target,
                builder.OriginalTarget,
                builder?.PreviousTokenBuilder?.Target,
                builder.Intensity.ToString(),
                "Apply/Remove",
                $"stack(s) of {builder.StatusEffect.Name}",
                builder.GetIntensityDescriptionIfNotConstant(),
                ComposeValueTargetLocation.AfterSuffix,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                (ICombatantTarget curValue) =>
                {
                    return !(builder.Target == builder.OriginalTarget && builder.Target == builder?.PreviousTokenBuilder?.Target
                    && builder.Target.IsFoeOf(builder.User)
                    && builder.Target.GetRepresentingNumberOfTargets() == 1);
                });
        }

        public static string DescribeResolvedStatusEffect(DeltaEntry delta)
        {
            return ComposeDescriptor<ICombatantTarget>(
                $"on {delta.Target.Name}",
                delta.Target,
                delta.OriginalTarget,
                delta.MadeFromBuilder?.PreviousTokenBuilder?.Target,
                delta.Intensity.ToString(),
                "Apply/Remove",
                $"stack(s) of {delta.StatusEffect.Name}",
                String.Empty,
                ComposeValueTargetLocation.AfterSuffix,
                ComposeValueTargetLocation.BetweenMiddleAndSuffix,
                null);
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

            bool shouldPutTarget = !(previoustarget != null && previoustarget.Equals(target));

            if (omitTargetDelegate != null && previoustarget == null && shouldPutTarget)
            {
                shouldPutTarget &= omitTargetDelegate(target);
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

            return builtString;
        }
        #endregion
    }
}