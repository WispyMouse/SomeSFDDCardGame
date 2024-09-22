namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class StatusEffectImport 
    {
        public enum StatusEffectPersistence
        {
            Combat = 1,
            Campaign = 2
        }

        public string Name;
        public string Id;
        public List<EffectOnProcImport> Effects = new List<EffectOnProcImport>();
        public StatusEffectPersistence Persistence = StatusEffectPersistence.Combat;
    }
}