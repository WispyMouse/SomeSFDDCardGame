namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class EnemyAttack : IAttackTokenHolder, IEffectOwner
    {
        public List<IScriptingToken> AttackTokens => this.AttackTokenPile.AttackTokens;
        public AttackTokenPile AttackTokenPile { get; set; }
        public ICombatantTarget PrecalculatedTarget { get; set; }
        public Dictionary<Element, int> BaseElementGain => new Dictionary<Element, int>();

        public IEffectOwner Owner => this;

        public string Id => string.Empty;

        public EnemyAttack(EnemyAttackImport basedOn)
        {
            this.AttackTokenPile = ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(basedOn.AttackScript, this);
        }
    }
}