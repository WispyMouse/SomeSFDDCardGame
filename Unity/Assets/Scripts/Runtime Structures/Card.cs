namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class Card : IAttackTokenHolder
    {
        public string Id;
        public string Name;
        public Sprite Sprite;

        public HashSet<string> Tags;
        public Dictionary<Element, int> BaseElementGain { get; set; } = new Dictionary<Element, int>();

        public List<IScriptingToken> AttackTokens => this.AttackTokenPile.AttackTokens;
        public AttackTokenPile AttackTokenPile { get; set; }

        public Card(CardImport basedOn)
        {
            this.Id = basedOn.Id.ToLower();
            this.Name = basedOn.Name;
            this.AttackTokenPile = ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(basedOn.EffectScript);

            HashSet<string> lowerCaseTags = new HashSet<string>();
            foreach (string tag in basedOn.Tags)
            {
                lowerCaseTags.Add(tag.ToLower());
            }

            foreach (ResourceGainImport gain in basedOn.ElementGain)
            {
                this.BaseElementGain.Add(ElementDatabase.GetElement(gain.Element), gain.Gain);
            }

            this.Tags = lowerCaseTags;
            this.Sprite = basedOn.Sprite;
        }

        public EffectDescription GetDescription()
        {
            return new EffectDescription()
            {
                MentionedStatusEffects = ScriptTokenEvaluator.GetMentionedStatusEffects(this),
                DescriptionText = new List<string>() { ScriptTokenEvaluator.DescribeCardText(this) },
                DescribingLabel = this.Name
            };
        }

        public int GetElementGain(Element element)
        {
            if (this.BaseElementGain.TryGetValue(element, out int gain))
            {
                return gain;
            }

            return 0;
        }
    }
}