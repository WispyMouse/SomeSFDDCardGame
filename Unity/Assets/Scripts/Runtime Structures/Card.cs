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
        public Sprite Sprite;

        public HashSet<string> Tags;
        public Dictionary<Element, int> BaseElementGain { get; set; } = new Dictionary<Element, int>();

        public List<IScriptingToken> AttackTokens { get; set; } = new List<IScriptingToken>();

        public Card Clone()
        {
            return new Card()
            {
                Id = this.Id,
                Name = this.Name,
                Sprite = this.Sprite,
                AttackTokens = new List<IScriptingToken>(AttackTokens),
                Tags = this.Tags,
                BaseElementGain = this.BaseElementGain
            };
        }

        public bool MeetsAllTags(HashSet<string> tags)
        {
            foreach (string tag in tags)
            {
                if (!this.Tags.Contains(tag))
                {
                    return false;
                }
            }

            return true;
        }

        public string GetDescription()
        {
            return ScriptTokenEvaluator.DescribeCardText(this);
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