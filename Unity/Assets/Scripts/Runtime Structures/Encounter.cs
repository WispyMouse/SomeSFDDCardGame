namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Encounter
    {
        public string Id;
        public List<string> EnemiesInEncounterById { get; set; } = new List<string>();

        public bool IsShopEncounter;

        public List<EnemyModel> GetEnemyModels()
        {
            List<EnemyModel> models = new List<EnemyModel>();

            foreach (string enemyId in this.EnemiesInEncounterById)
            {
                models.Add(EnemyDatabase.GetModel(enemyId));
            }

            return models;
        }
    }
}