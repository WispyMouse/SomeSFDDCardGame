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
        public Dictionary<Element, int> BaseElementGain => new Dictionary<Element, int>();

        public IEffectOwner Owner => this.OwnedStatusEffect.BasedOnStatusEffect;

        public StatusEffectHappening(AppliedStatusEffect ownedStatusEffect, List<IScriptingToken> attackTokens)
        {
            this.AttackTokens = attackTokens;
            this.OwnedStatusEffect = ownedStatusEffect;
        }
    }
}