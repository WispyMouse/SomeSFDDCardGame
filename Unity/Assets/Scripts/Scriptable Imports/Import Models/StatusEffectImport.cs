namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class StatusEffectImport 
    {
        public string Name;
        public string Id;
        public List<EffectOnProcImport> Effects = new List<EffectOnProcImport>();
    }
}