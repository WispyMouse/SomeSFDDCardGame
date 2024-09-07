namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    [Serializable]
    public class CardImport
    {
        public string Id;
        public string Name;
        public string EffectScript;

        public Card DeriveCard()
        {
            Card card = new Card();

            card.Id = Id;
            card.Name = Name;
            card.AttackTokens = ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(this.EffectScript);
            card.EffectText = ScriptTokenEvaluator.DescribeCardText(card);

            return card;
        }
    }
}