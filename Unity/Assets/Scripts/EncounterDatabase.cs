namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class EncounterDatabase
    {
        public static Dictionary<string, Encounter> EncounterData { get; set; } = new Dictionary<string, Encounter>();

        public static void AddEncounter(EncounterImport toAdd)
        {
            EncounterData.Add(toAdd.Id, toAdd.DeriveEncounter());
        }

        public static Encounter GetRandomEncounter()
        {
            List<string> modelIds = new List<string>(EncounterData.Keys);
            int randomIndex = UnityEngine.Random.Range(0, modelIds.Count);
            return EncounterData[modelIds[randomIndex]];
        }
    }
}