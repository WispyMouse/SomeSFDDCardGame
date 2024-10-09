namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using static SFDDCards.ImportModels.StatusEffectImport;

    public class StatusEffect : IStatusEffect, IEffectOwner, IEquatable<StatusEffect>
    {
        public readonly string Name;
        public readonly string Id;
        public readonly Dictionary<string, List<WindowResponder>> WindowResponders = new Dictionary<string, List<WindowResponder>>();
        public readonly Sprite Sprite;
        public readonly StatusEffectPersistence Persistence = StatusEffectPersistence.Combat;
        public readonly HashSet<string> Tags = new HashSet<string>();

        string IEffectOwner.Id => this.Id;

        public StatusEffect(StatusEffectImport basedOn)
        {
            this.Name = basedOn.Name;
            this.Id = basedOn.Id;
            this.Sprite = basedOn.StatusEffectArt;
            this.Persistence = basedOn.Persistence;
            this.Tags = basedOn.Tags;

            foreach (EffectOnProcImport import in basedOn.Effects)
            {
                if (!this.WindowResponders.TryGetValue(import.Window.ToLower(), out List<WindowResponder> responses))
                {
                    responses = new List<WindowResponder>();
                    this.WindowResponders.Add(import.Window.ToLower(), responses);
                }

                responses.Add(new WindowResponder()
                {
                    ApplicationPriority = import.ApplicationPriority,
                    WindowId = import.Window,
                    Effect = ScriptingTokenDatabase.GetAllTokens(import.Script, this)
                });
            }
        }

        public EffectDescription DescribeStatusEffect()
        {
            List<string> statusEffects = new List<string>();

            foreach (string window in this.WindowResponders.Keys)
            {
                foreach (WindowResponder responder in this.WindowResponders[window])
                {
                    StringBuilder thisWindowString = new StringBuilder();

                    string windowDescription = KnownReactionWindows.GetWindowDescriptor(window.ToLower());

                    if (!string.IsNullOrEmpty(windowDescription))
                    {
                        thisWindowString.Append($"<b>{windowDescription}:</b>");
                    }

                    ConceptualTokenEvaluatorBuilder previousRequirementsBuilder = null;
                    List<ConceptualTokenEvaluatorBuilder> tokenEvaluators = ScriptTokenEvaluator.CalculateConceptualBuildersFromTokenEvaluation(responder.Effect);
                    foreach (ConceptualTokenEvaluatorBuilder builder in tokenEvaluators)
                    {
                        ConceptualDelta delta = builder.GetConceptualDelta();
                        if (delta.DeltaEntries.Count > 0)
                        {
                            if (!builder.HasSameRequirements(previousRequirementsBuilder))
                            {
                                thisWindowString.Append(EffectDescriberDatabase.DescribeRequirement(builder));
                                previousRequirementsBuilder = builder;
                            }

                            thisWindowString.Append($"{EffectDescriberDatabase.DescribeConceptualEffect(delta, window.ToLower())} ");
                        }
                    }

                    statusEffects.Add(thisWindowString.ToString().Trim());
                }
            }

            HashSet<StatusEffect> mentionedEffects = new HashSet<StatusEffect>();

            foreach (string window in this.WindowResponders.Keys)
            {
                foreach (WindowResponder responder in this.WindowResponders[window])
                {
                    mentionedEffects.UnionWith(ScriptTokenEvaluator.GetMentionedStatusEffects(responder.Effect));
                }
            }

            return new EffectDescription()
            {
                MentionedStatusEffects = mentionedEffects,
                DescriptionText = statusEffects,
                DescribingLabel = this.Name
            };
        }

        public bool Equals(StatusEffect other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.Id.Equals(this.Id))
            {
                return true;
            }

            return false;
        }

        public bool MeetsAllTags(HashSet<string> tags)
        {
            foreach (string tag in tags)
            {
                if (!this.Tags.Contains(tag))
                {
                    return false;
                }
            }

            return true;
        }
    }
}