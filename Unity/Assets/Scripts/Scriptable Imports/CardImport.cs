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
        public HashSet<string> Tags = new HashSet<string>();
        public List<ResourceGainImport> ElementGain = new List<ResourceGainImport>();

        public Card DeriveCard()
        {
            Card card = new Card();

            card.Id = Id.ToLower();
            card.Name = Name;
            card.AttackTokens = ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(this.EffectScript);

            HashSet<string> lowerCaseTags = new HashSet<string>();
            foreach (string tag in this.Tags)
            {
                lowerCaseTags.Add(tag.ToLower());
            }

            foreach (ResourceGainImport gain in this.ElementGain)
            {
                card.BaseElementGain.Add(ElementDatabase.GetElement(gain.Element), gain.Gain);
            }

            card.Tags = lowerCaseTags;

            return card;
        }
    }
}