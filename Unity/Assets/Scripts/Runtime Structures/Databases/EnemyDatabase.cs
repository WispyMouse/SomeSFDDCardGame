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

            EnemyModel newEnemy = new EnemyModel();
            newEnemy.Id = lowerId;
            newEnemy.Name = importData.Name;
            newEnemy.MaximumHealth = importData.MaximumHealth;

            foreach (EnemyAttackImport attack in importData.Attacks)
            {
                newEnemy.Attacks.Add(attack.DeriveAttack());
            }

            EnemyData.Add(lowerId, newEnemy);
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
    }
}