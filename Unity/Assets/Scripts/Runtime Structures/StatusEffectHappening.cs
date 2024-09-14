namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;


    public struct StatusEffectHappening : IAttackTokenHolder
    {
        public List<IScriptingToken> AttackTokens { get; set; }
        public AppliedStatusEffect OwnedStatusEffect { get; set; }

        public StatusEffectHappening(AppliedStatusEffect ownedStatusEffect, List<IScriptingToken> attackTokens)
        {
            this.AttackTokens = attackTokens;
            this.OwnedStatusEffect = ownedStatusEffect;
        }
    }
}