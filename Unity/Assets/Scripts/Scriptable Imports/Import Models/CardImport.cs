namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class CardImport
    {
        public string Id;
        public string Name;
        public string EffectScript;
        public HashSet<string> Tags = new HashSet<string>();
        public List<ResourceGainImport> ElementGain = new List<ResourceGainImport>();

        [NonSerialized]
        public Sprite Sprite;

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
    }
}