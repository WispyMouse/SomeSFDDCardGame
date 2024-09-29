namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
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
            string spriteFile = new FileInfo(this.FilePath).Extension.ToLower().Replace("statusimport", "png");

            if (File.Exists(spriteFile))
            {
                this.StatusEffectArt = await ImportHelper.GetSpriteAsync(spriteFile, 64, 64);
            }
        }
    }
}