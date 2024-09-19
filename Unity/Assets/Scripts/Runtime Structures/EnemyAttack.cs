namespace SFDDCards
{
    using SFDDCards.ImportModels;
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
        public Dictionary<Element, int> BaseElementGain => new Dictionary<Element, int>();

        public EnemyAttack(EnemyAttackImport basedOn)
        {
            this.AttackTokens = ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(basedOn.AttackScript);
        }
    }
}