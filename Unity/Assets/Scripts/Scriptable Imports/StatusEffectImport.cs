namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    [Serializable]
    public class StatusEffectImport 
    {
        public string Name;
        public string Id;
        public List<EffectOnProcImport> Effects;

        public StatusEffect DeriveStatusEffect()
        {
            StatusEffect newEffect = new StatusEffect();

            newEffect.Name = this.Name;
            newEffect.Id = this.Id.ToLower();

            foreach (EffectOnProcImport import in this.Effects)
            {
                if (!newEffect.EffectTokens.TryGetValue(import.Window.ToLower(), out List<List<ScriptingTokens.IScriptingToken>> tokens))
                {
                    tokens = new List<List<ScriptingTokens.IScriptingToken>>();
                    newEffect.EffectTokens.Add(import.Window.ToLower(), tokens);
                }

                tokens.Add(ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(import.Script));
            }

            return newEffect;
        }
    }
}