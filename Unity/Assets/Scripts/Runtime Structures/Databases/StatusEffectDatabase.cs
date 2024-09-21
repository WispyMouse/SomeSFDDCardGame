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

        public static void AddStatusEffectToDatabase(StatusEffectImport importData, Sprite  statusArt = null)
        {
            string lowerId = importData.Id.ToLower();

            if (EffectData.ContainsKey(lowerId))
            {
                Debug.LogError($"EffectData dictionary already contains id {lowerId}");
                return;
            }

            EffectData.Add(importData.Id.ToLower(), new StatusEffect(importData, statusArt));
        }

        public static bool TryImportStatusEffectFromFile(string filepath, out StatusEffect output, Sprite statusArt = null)
        {
            GlobalUpdateUX.LogTextEvent.Invoke($"Loading and parsing {filepath}...", GlobalUpdateUX.LogType.GameEvent);

            try
            {
                string fileText = File.ReadAllText(filepath);
                StatusEffectImport importedStatusEffect = Newtonsoft.Json.JsonConvert.DeserializeObject<StatusEffectImport>(fileText);

                string artLocation = $"{filepath.ToLower().Replace(".statusimport", ".png")}";
                Sprite elementArt = null;
                if (File.Exists(artLocation))
                {
                    byte[] imageBytes = File.ReadAllBytes(artLocation);
                    Texture2D texture = new Texture2D(64, 64);
                    texture.LoadImage(imageBytes);
                    elementArt = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.zero);
                }
                else
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Could not find art for {filepath} at expected location of {artLocation}", GlobalUpdateUX.LogType.Info);
                }

                StatusEffectDatabase.AddStatusEffectToDatabase(importedStatusEffect, statusArt);
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