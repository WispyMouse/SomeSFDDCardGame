namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using static SFDDCards.ImportModels.IImportable;

    [System.Serializable]
    public abstract class Importable : IImportable
    {
        string IImportable.Id
        {
            get
            {
                return this.Id;
            }
        }

        public string Id;
        public string FilePath { get; set; }

        public virtual async Task ProcessAdditionalFilesAsync()
        {
            await Task.CompletedTask;
        }

        public virtual void ProcessAdditionalFiles()
        {
        }
    }
}