namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;


    public class StatusEffectHappening : IAttackTokenHolder
    {
        public List<IScriptingToken> AttackTokens { get; set; } = new List<IScriptingToken>();
    }
}