namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public class TextMeshProSpriteController : MonoBehaviour
    {
        [SerializeReference]
        private Texture2D WriteableTexture;

        private int CurrentSpriteIndex { get; set; } = 0;
        private int SpriteWidth { get; } = 64;
        private int SpriteHeight { get; } = 64;

        private int TextureWidth => this.WriteableTexture.width;
        private int TextureHeight => this.WriteableTexture.height;
        private int SpritesPerRow => this.TextureWidth / this.SpriteWidth;

        public int AddSprite(Sprite toAdd)
        {
            int assignedSpriteIndex = this.CurrentSpriteIndex;

            int x = this.SpriteWidth * (assignedSpriteIndex % this.SpritesPerRow);
            int y = this.TextureHeight - this.SpriteHeight - this.SpriteHeight * Mathf.FloorToInt(assignedSpriteIndex / this.SpritesPerRow);

            for (int xx = 0; xx < SpriteWidth; xx++)
            {
                for (int yy = 0; yy < SpriteHeight; yy++)
                {
                    Color getPixel = toAdd.texture.GetPixel(xx, yy);
                    WriteableTexture.SetPixel(xx + x, yy + y, getPixel);
                }
            }

            WriteableTexture.Apply();
            this.CurrentSpriteIndex++;

            return assignedSpriteIndex;
        }
    }
}