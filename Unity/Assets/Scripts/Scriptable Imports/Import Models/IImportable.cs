namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IImportable
    {
        public string FilePath { get; set; }
        public string Id { get; }
        public Task ProcessAdditionalFilesAsync();
    }
}