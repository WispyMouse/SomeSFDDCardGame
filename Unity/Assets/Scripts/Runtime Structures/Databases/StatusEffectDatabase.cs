namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public static class StatusEffectDatabase
    {
        public static Dictionary<string, StatusEffect> EffectData { get; private set; } = new Dictionary<string, StatusEffect>();

        public static void AddStatusEffectToDatabase(StatusEffectImport importData)
        {
            string lowerId = importData.Id.ToLower();

            if (EffectData.ContainsKey(lowerId))
            {
                Debug.LogError($"EffectData dictionary already contains id {lowerId}");
                return;
            }

            EffectData.Add(importData.Id.ToLower(), new StatusEffect(importData));
        }

        public static bool TryImportStatusEffectFromFile(string filepath, out StatusEffect output)
        {
            GlobalUpdateUX.LogTextEvent.Invoke($"Loading and parsing {filepath}...", GlobalUpdateUX.LogType.GameEvent);

            try
            {
                string fileText = File.ReadAllText(filepath);
                StatusEffectImport importedStatusEffect = Newtonsoft.Json.JsonConvert.DeserializeObject<StatusEffectImport>(fileText);
                StatusEffectDatabase.AddStatusEffectToDatabase(importedStatusEffect);
                output = GetModel(importedStatusEffect.Id);
                return true;
            }
            catch (Exception e)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse! Debug log has exception details.", GlobalUpdateUX.LogType.RuntimeError);
                Debug.LogException(e);
                output = null;
                return false;
            }
        }

        public static StatusEffect GetModel(string id)
        {
            string lowerId = id.ToLower();

            if (!EffectData.TryGetValue(lowerId, out StatusEffect foundModel))
            {
                Debug.LogError($"EffectData dictionary lookup does not contain id {lowerId}");
            }

            return foundModel;
        }

        public static void ClearDatabase()
        {
            EffectData.Clear();
        }
    }
}