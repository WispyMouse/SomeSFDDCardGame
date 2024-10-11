namespace SFDDCards.ImportModels
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Threading;
    using UnityEngine;

    [Serializable]
    public class ElementImport : Importable
    {
        public string Name;

        [NonSerialized]
        public Sprite NormalArt;

        [NonSerialized]
        public Sprite GreyscaleArt;

        public override async Task ProcessAdditionalFilesAsync(SynchronizationContext mainThreadContext)
        {
            string normalArtFile = this.FilePath.ToLower().Replace("elementimport", "png");

            if (File.Exists(normalArtFile))
            {
                this.NormalArt = await ImportHelper.GetSpriteAsync(normalArtFile, 64, 64, mainThreadContext).ConfigureAwait(false);
            }

            string greyscaleFile = new FileInfo(this.FilePath).Extension.ToLower().Replace("elementimport", "greyscale.png");

            if (File.Exists(greyscaleFile))
            {
                this.GreyscaleArt = await ImportHelper.GetSpriteAsync(greyscaleFile, 64, 64, mainThreadContext).ConfigureAwait(false);
            }
        }

        public override void ProcessAdditionalFiles()
        {
            string normalArtFile = this.FilePath.ToLower().Replace("elementimport", "png");

            if (File.Exists(normalArtFile))
            {
                this.NormalArt = ImportHelper.GetSprite(normalArtFile, 64, 64);
            }

            string greyscaleFile = new FileInfo(this.FilePath).Extension.ToLower().Replace("elementimport", "greyscale.png");

            if (File.Exists(greyscaleFile))
            {
                this.GreyscaleArt = ImportHelper.GetSprite(greyscaleFile, 64, 64);
            }
        }
    }
}