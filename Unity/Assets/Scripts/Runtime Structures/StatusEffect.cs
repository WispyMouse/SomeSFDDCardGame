namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using static SFDDCards.ImportModels.StatusEffectImport;

    public class StatusEffect : IStatusEffect
    {
        public readonly string Name;
        public readonly string Id;
        public readonly Dictionary<string, List<AttackTokenPile>> EffectTokens = new Dictionary<string, List<AttackTokenPile>>();
        public readonly Sprite Sprite;
        public readonly StatusEffectPersistence Persistence = StatusEffectPersistence.Combat;
        public readonly HashSet<string> Tags = new HashSet<string>();

        public StatusEffect(StatusEffectImport basedOn, Sprite sprite = null)
        {
            this.Name = basedOn.Name;
            this.Id = basedOn.Id;
            this.Sprite = sprite;
            this.Persistence = basedOn.Persistence;
            this.Tags = basedOn.Tags;

            foreach (EffectOnProcImport import in basedOn.Effects)
            {
                if (!this.EffectTokens.TryGetValue(import.Window.ToLower(), out List<AttackTokenPile> tokens))
                {
                    tokens = new List<AttackTokenPile>();
                    this.EffectTokens.Add(import.Window.ToLower(), tokens);
                }

                tokens.Add(ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(import.Script));
            }
        }

        public EffectDescription DescribeStatusEffect()
        {
            List<string> statusEffects = new List<string>();

            foreach (string window in this.EffectTokens.Keys)
            {
                StringBuilder thisWindowString = new StringBuilder();

                string windowDescription = KnownReactionWindows.GetWindowDescriptor(window.ToLower());
                thisWindowString.Append($"<b>{windowDescription}:</b> ");

                foreach (AttackTokenPile attackTokenList in this.EffectTokens[window])
                {
                    List<ConceptualTokenEvaluatorBuilder> tokenEvaluators = ScriptTokenEvaluator.CalculateConceptualBuildersFromTokenEvaluation(attackTokenList);
                    foreach (ConceptualTokenEvaluatorBuilder builder in tokenEvaluators)
                    {
                        thisWindowString.Append($"{EffectDescriberDatabase.DescribeConceptualEffect(builder.GetConceptualDelta())} ");
                    }
                }

                statusEffects.Add(thisWindowString.ToString());
            }

            HashSet<StatusEffect> mentionedEffects = new HashSet<StatusEffect>();
            foreach (List<AttackTokenPile> piles in this.EffectTokens.Values)
            {
                foreach (AttackTokenPile curPile in piles)
                {
                    mentionedEffects.UnionWith(ScriptTokenEvaluator.GetMentionedStatusEffects(curPile));
                }
            }

            return new EffectDescription()
            {
                MentionedStatusEffects = mentionedEffects,
                DescriptionText = statusEffects,
                DescribingLabel = this.Name
            };
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