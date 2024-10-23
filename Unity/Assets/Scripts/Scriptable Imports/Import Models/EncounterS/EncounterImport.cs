namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class EncounterImport : Importable
    {
        public string Name;
        public string Description;
        public List<string> EnemyIds = new List<string>();
        public bool IsShopEncounter;
        public HashSet<string> Tags = new HashSet<string>();
        public List<string> Arguments = new List<string>();

        public RewardImport CustomReward = null;
        public string StandardRewardId;

        public List<EncounterScriptImport> EncounterScripts = null;
    }
}