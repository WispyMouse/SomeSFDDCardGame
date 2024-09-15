namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class EncounterImport
    {
        public string Id;
        public string Name;
        public string Description;
        public List<string> EnemyIds = new List<string>();
        public bool IsShopEncounter;
        public HashSet<string> EncounterTags = new HashSet<string>();
        public List<string> Arguments = new List<string>();

        public EncounterModel DeriveEncounter()
        {
            HashSet<string> lowerCaseTags = new HashSet<string>();
            foreach (string tag in this.EncounterTags)
            {
                lowerCaseTags.Add(tag.ToLower());
            }

            List<string> lowerCaseEnemyIds = new List<string>();
            foreach (string enemyId in this.EnemyIds)
            {
                lowerCaseEnemyIds.Add(enemyId.ToLower());
            }

            return new EncounterModel()
            {
                Id = this.Id.ToLower(),
                Name = this.Name,
                EncounterTags = lowerCaseTags,
                Description = this.Description,
                EnemiesInEncounterById = lowerCaseEnemyIds,
                IsShopEncounter = this.IsShopEncounter,
                Arguments = this.Arguments
            };
        }
    }
}