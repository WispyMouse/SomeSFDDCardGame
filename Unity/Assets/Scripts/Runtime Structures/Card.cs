namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class Card : IAttackTokenHolder, IEffectOwner
    {
        public enum KnownRarities
        {
            Unknown,
            Starter,
            Common,
            Uncommon,
            Rare,
            Generated
        }

        string IEffectOwner.Id => this.Id;

        public string Id;
        public string Name;
        public Sprite Sprite;

        public HashSet<string> Tags;
        public KnownRarities Rarity;
        
        public Dictionary<Element, int> BaseElementGain { get; set; } = new Dictionary<Element, int>();

        public List<IScriptingToken> AttackTokens => this.AttackTokenPile.AttackTokens;
        public AttackTokenPile AttackTokenPile { get; set; }

        public IEffectOwner Owner => this;
        public CardImport BasedOn;

        public Card(CardImport basedOn)
        {
            this.BasedOn = basedOn;
            this.Id = basedOn.Id.ToLower();
            this.Name = basedOn.Name;

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
            this.Sprite = basedOn.CardArt;

            this.AttackTokenPile = ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(basedOn.EffectScript, this);

            foreach (KnownRarities knownRarity in Enum.GetValues(typeof(KnownRarities)))
            {
                string enumName = Enum.GetName(typeof(KnownRarities), knownRarity).ToLower();
                foreach (string tag in this.Tags)
                {
                    if (enumName == tag.ToLower())
                    {
                        this.Rarity = knownRarity;
                        break;
                    }
                }
            }
        }

        public EffectDescription GetDescription(ReactionWindowContext? context = null)
        {
            return new EffectDescription()
            {
                MentionedStatusEffects = ScriptTokenEvaluator.GetMentionedStatusEffects(this),
                DescriptionText = new List<string>() { ScriptTokenEvaluator.DescribeCardText(this, context) },
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