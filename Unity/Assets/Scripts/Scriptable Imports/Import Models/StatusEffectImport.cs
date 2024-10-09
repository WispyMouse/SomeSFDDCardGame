namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Threading;
    using UnityEngine;

    [Serializable]
    public class StatusEffectImport : Importable
    {
        public enum StatusEffectPersistence
        {
            Combat = 1,
            Campaign = 2
        }

        public string Name;
        public List<EffectOnProcImport> Effects = new List<EffectOnProcImport>();
        public StatusEffectPersistence Persistence = StatusEffectPersistence.Combat;
        public HashSet<string> Tags = new HashSet<string>();

        [NonSerialized]
        public Sprite StatusEffectArt;

        public override async Task ProcessAdditionalFilesAsync()
        {
            string spriteFile = this.FilePath.ToLower().Replace("statusimport", "png");

            if (File.Exists(spriteFile))
            {
                this.StatusEffectArt = await ImportHelper.GetSpriteAsync(spriteFile, 64, 64).ConfigureAwait(false);
            }
        }

        public override void ProcessAdditionalFiles()
        {
            string spriteFile = this.FilePath.ToLower().Replace("statusimport", "png");

            if (File.Exists(spriteFile))
            {
                this.StatusEffectArt = ImportHelper.GetSprite(spriteFile, 64, 64);
            }
        }
    }
}