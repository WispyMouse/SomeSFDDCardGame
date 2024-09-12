namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class StatusEffect
    {
        public string Name;
        public string Id;
        public Dictionary<string, List<ScriptingTokens.IScriptingToken>> EffectTokens = new Dictionary<string, List<ScriptingTokens.IScriptingToken>>();

        public bool TryGetHappening(CentralGameStateController gameStateController, string window, out StatusEffectHappening happening)
        {
            happening = null;

            if (!this.EffectTokens.TryGetValue(window, out List<ScriptingTokens.IScriptingToken> tokens))
            {
                return false;
            }

            happening = new StatusEffectHappening()
            {
                AttackTokens = tokens
            };

            return true;
        }
    }
}