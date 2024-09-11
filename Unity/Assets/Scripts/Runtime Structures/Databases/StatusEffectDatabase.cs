namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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

            EffectData.Add(importData.Id.ToLower(), importData.DeriveStatusEffect());
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
    }
}