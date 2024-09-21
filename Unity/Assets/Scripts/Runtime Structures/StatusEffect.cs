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
        public Dictionary<string, List<List<ScriptingTokens.IScriptingToken>>> EffectTokens = new Dictionary<string, List<List<ScriptingTokens.IScriptingToken>>>();
        public Sprite Sprite;

        public StatusEffect(StatusEffectImport basedOn, Sprite sprite = null)
        {
            this.Name = basedOn.Name;
            this.Id = basedOn.Id;
            this.Sprite = sprite;

            foreach (EffectOnProcImport import in basedOn.Effects)
            {
                if (!this.EffectTokens.TryGetValue(import.Window.ToLower(), out List<List<ScriptingTokens.IScriptingToken>> tokens))
                {
                    tokens = new List<List<ScriptingTokens.IScriptingToken>>();
                    this.EffectTokens.Add(import.Window.ToLower(), tokens);
                }

                tokens.Add(ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(import.Script));
            }
        }

        public List<string> DescribeStatusEffect()
        {
            List<string> statusEffects = new List<string>();

            foreach (string window in this.EffectTokens.Keys)
            {
                StringBuilder thisWindowString = new StringBuilder();

                string windowDescription = KnownReactionWindows.GetWindowDescriptor(window.ToLower());
                thisWindowString.Append($"<b>{windowDescription}:</b> ");

                foreach (List<IScriptingToken> attackTokenList in this.EffectTokens[window])
                {
                    List<TokenEvaluatorBuilder> tokenEvaluators = ScriptTokenEvaluator.CalculateEvaluatorBuildersFromTokenEvaluation(
                        new AttackTokenPile(attackTokenList));
                    foreach (TokenEvaluatorBuilder builder in tokenEvaluators)
                    {
                        thisWindowString.Append($"{builder.GetAbstractDelta().DescribeAsEffect()} ");
                    }
                }

                statusEffects.Add(thisWindowString.ToString());
            }

            return statusEffects;
        }
    }
}