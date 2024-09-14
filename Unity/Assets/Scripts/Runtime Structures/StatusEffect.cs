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
        public Dictionary<string, List<List<ScriptingTokens.IScriptingToken>>> EffectTokens = new Dictionary<string, List<List<ScriptingTokens.IScriptingToken>>>();
    }
}