namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class EnemyAttack : IAttackTokenHolder
    {
        public List<IScriptingToken> AttackTokens { get; set; }
        public ICombatantTarget PrecalculatedTarget { get; set; }
    }
}