namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;


    public struct StatusEffectHappening : IAttackTokenHolder
    {
        public WindowResponse Response { get; set; }
        public AppliedStatusEffect OwnedStatusEffect { get; set; }
        public ReactionWindowContext Context { get; set; }
        public Dictionary<Element, int> BaseElementGain => new Dictionary<Element, int>();

        public IEffectOwner Owner => this.OwnedStatusEffect.BasedOnStatusEffect;

        public List<IScriptingToken> AttackTokens => this.Response.FromResponder?.Effect?.AttackTokens;

        public StatusEffectHappening(WindowResponse response)
        {
            this.Response = response;
            this.Context = response.FromContext;
            this.OwnedStatusEffect = response.FromStatusEffect;
        }
    }
}