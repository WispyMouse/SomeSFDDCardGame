namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class EncounterModel
    {
        public string Id;

        public string Name;
        public string Description;
        public HashSet<string> EncounterTags { get; set; } = new HashSet<string>();

        public List<string> EnemiesInEncounterById { get; set; } = new List<string>();

        public bool IsShopEncounter;
        public List<string> Arguments { get; set; } = new List<string>();

        public List<EnemyModel> GetEnemyModels()
        {
            List<EnemyModel> models = new List<EnemyModel>();
            DoNotRepeatRandomDecider<EnemyModel> dontRepeatDecider = new DoNotRepeatRandomDecider<EnemyModel>();

            foreach (string enemyId in this.EnemiesInEncounterById)
            {
                models.Add(EnemyDatabase.GetModel(enemyId, dontRepeatDecider));
            }

            return models;
        }

        public bool MeetsAllTags(HashSet<string> tags)
        {
            return this.EncounterTags.Overlaps(tags);
        }
    }
}