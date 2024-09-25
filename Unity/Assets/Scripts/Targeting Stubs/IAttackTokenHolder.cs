namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public interface IAttackTokenHolder
    {
        public List<IScriptingToken> AttackTokens { get; }
        public Dictionary<Element, int> BaseElementGain { get; }
        public IEffectOwner Owner { get; }
    }
}