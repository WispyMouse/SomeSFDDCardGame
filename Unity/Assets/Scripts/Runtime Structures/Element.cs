namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Element
    {
        public string Id;
        public string Name;
        public Sprite Sprite;
        public int? SpriteIndex;
        public Sprite GreyscaleSprite;

        public string GetNameOrIcon()
        {
            if (this.SpriteIndex.HasValue)
            {
                return $"<sprite index={this.SpriteIndex.Value}>";
            }

            return this.Name;
        }

        public string GetNameAndMaybeIcon()
        {
            if (this.SpriteIndex.HasValue)
            {
                return $"<sprite index={this.SpriteIndex.Value}>{this.Name}";
            }

            return this.Name;
        }
    }
}