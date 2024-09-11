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
        public string Id;
        public List<EffectOnProcImport> Effects;

        public StatusEffect DeriveStatusEffect()
        {
            StatusEffect newEffect = new StatusEffect();

            newEffect.Id = this.Id.ToLower();

            foreach (EffectOnProcImport import in this.Effects)
            {
                if (!newEffect.EffectTokens.TryGetValue(import.Window.ToLower(), out List<ScriptingTokens.IScriptingToken> tokens))
                {
                    tokens = new List<ScriptingTokens.IScriptingToken>();
                    newEffect.EffectTokens.Add(import.Window.ToLower(), tokens);
                }
                else
                {
                    // If there are more scripting tokens to add to this window, make sure to separate them with a [RESET]
                    // that way effects don't bleed context into eachother
                    tokens.Add(new ScriptingTokens.ResetScriptingToken());
                }

                tokens.AddRange(ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(import.Script));
            }

            return newEffect;
        }
    }
}