namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class StatusEffect
    {
        public string Name;
        public string Id;
        public Dictionary<string, List<List<ScriptingTokens.IScriptingToken>>> EffectTokens = new Dictionary<string, List<List<ScriptingTokens.IScriptingToken>>>();

        public StatusEffect(StatusEffectImport basedOn)
        {
            this.Name = basedOn.Name;
            this.Id = basedOn.Id;

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
    }
}