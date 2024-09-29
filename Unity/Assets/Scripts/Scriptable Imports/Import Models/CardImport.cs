namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;

    [Serializable]
    public class CardImport : Importable
    {
        public string Name;
        public string EffectScript;
        public HashSet<string> Tags = new HashSet<string>();
        public List<ResourceGainImport> ElementGain = new List<ResourceGainImport>();

        [System.NonSerialized]
        public Sprite CardArt;

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

        public override async Task ProcessAdditionalFilesAsync()
        {
            string spriteFile = new FileInfo(this.FilePath).Extension.ToLower().Replace("cardimport", "png");

            if (File.Exists(spriteFile))
            {
                this.CardArt = await ImportHelper.GetSpriteAsync(spriteFile, 144, 80);
            }
        }
    }
}