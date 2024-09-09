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
        public List<string> EnemyIds = new List<string>();
        public bool IsShopEncounter;
        public Encounter DeriveEncounter()
        {
            return new Encounter()
            {
                Id = this.Id.ToLower(),
                EnemiesInEncounterById = new List<string>(this.EnemyIds),
                IsShopEncounter = this.IsShopEncounter
            };
        }
    }
}