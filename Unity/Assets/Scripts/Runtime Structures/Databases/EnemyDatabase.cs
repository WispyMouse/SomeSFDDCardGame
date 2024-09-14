namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class EnemyDatabase
    {
        public static Dictionary<string, EnemyModel> EnemyData { get; private set; } = new Dictionary<string, EnemyModel>();

        public static void AddEnemyToDatabase(EnemyImport importData)
        {
            string lowerId = importData.Id.ToLower();

            if (EnemyData.ContainsKey(lowerId))
            {
                Debug.LogError($"EnemyData dictionary already contains id {lowerId}");
                return;
            }

            EnemyData.Add(lowerId, importData.DeriveEnemyModel());
        }

        public static EnemyModel GetModel(string id)
        {
            string lowerId = id.ToLower();

            if (!EnemyData.TryGetValue(lowerId, out EnemyModel foundModel))
            {
                Debug.LogError($"EnemyData dictionary lookup does not contain id {lowerId}");
            }

            return foundModel;
        }

        public static EnemyModel GetRandomModel()
        {
            List<string> modelIds = new List<string>(EnemyData.Keys);
            int randomIndex = UnityEngine.Random.Range(0, modelIds.Count);
            return GetModel(modelIds[randomIndex]);
        }

        public static void ClearDatabase()
        {
            EnemyData.Clear();
        }
    }
}