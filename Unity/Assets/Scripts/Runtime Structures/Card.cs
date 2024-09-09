namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class Card : IAttackTokenHolder
    {
        public string Id;
        public string Name;
        public string EffectText;
        public Sprite Sprite;

        public List<IScriptingToken> AttackTokens { get; set; }

        public Card Clone()
        {
            return new Card()
            {
                Id = this.Id,
                Name = this.Name,
                Sprite = this.Sprite,
                EffectText = this.EffectText,
                AttackTokens = new List<IScriptingToken>(AttackTokens)
            };
        }
    }
}