namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    [Serializable]
    public class CurrencyImport : Importable
    {
        public string Name;

        [System.NonSerialized]
        public Sprite CurrencyArt;

        [System.NonSerialized]
        public int? SpriteIndex;

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

        public override async Task ProcessAdditionalFilesAsync(SynchronizationContext mainThreadContext)
        {
            string spriteFile = this.FilePath.ToLower().Replace("currencyimport", "png");

            if (File.Exists(spriteFile))
            {
                this.CurrencyArt = await ImportHelper.GetSpriteAsync(spriteFile, 64, 64, mainThreadContext).ConfigureAwait(false);
            }
        }

        public override void ProcessAdditionalFiles()
        {
            string spriteFile = this.FilePath.ToLower().Replace("currencyimport", "png");

            if (File.Exists(spriteFile))
            {
                this.CurrencyArt = ImportHelper.GetSprite(spriteFile, 64, 64);
            }
        }
    }
}