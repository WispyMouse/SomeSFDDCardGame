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

        public static Encounter GetRandomEncounter(params Encounter[] doNotInclude)
        {
            List<string> modelIds = new List<string>(EncounterData.Keys);

            if (modelIds.Count == 0)
            {
                Debug.LogError($"Somehow there are no valid encounters.");
                return null;
            }

            foreach (Encounter curEncounter in doNotInclude)
            {
                if (curEncounter != null)
                {
                    modelIds.Remove(curEncounter.Id);
                }
            }

            if (modelIds.Count == 0)
            {
                Debug.LogError($"There are no encounters left over after excluding all in provided list. Getting random without exclusions.");
                return GetRandomEncounter();
            }

            int randomIndex = UnityEngine.Random.Range(0, modelIds.Count);
            return EncounterData[modelIds[randomIndex]];
        }
    }
}