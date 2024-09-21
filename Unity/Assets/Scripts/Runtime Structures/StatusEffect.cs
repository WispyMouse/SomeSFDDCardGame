namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;

    public class StatusEffect
    {
        public string Name;
        public string Id;
        public Dictionary<string, List<AttackTokenPile>> EffectTokens = new Dictionary<string, List<AttackTokenPile>>();
        public Sprite Sprite;

        public StatusEffect(StatusEffectImport basedOn, Sprite sprite = null)
        {
            this.Name = basedOn.Name;
            this.Id = basedOn.Id;
            this.Sprite = sprite;

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
                    List<TokenEvaluatorBuilder> tokenEvaluators = ScriptTokenEvaluator.CalculateEvaluatorBuildersFromTokenEvaluation(
                        attackTokenList);
                    foreach (TokenEvaluatorBuilder builder in tokenEvaluators)
                    {
                        thisWindowString.Append($"{builder.GetConceptualDelta().DescribeDelta()} ");
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
    }
}