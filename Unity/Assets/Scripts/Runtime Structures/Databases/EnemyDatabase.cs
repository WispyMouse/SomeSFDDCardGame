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
            if (EnemyData.ContainsKey(importData.Id))
            {
                Debug.LogError($"EnemyData dictionary already contains id {importData.Id}");
                return;
            }

            EnemyModel newEnemy = new EnemyModel();
            newEnemy.Name = importData.Name;
            newEnemy.MaximumHealth = importData.MaximumHealth;

            foreach (EnemyAttackImport attack in importData.Attacks)
            {
                newEnemy.Attacks.Add(attack.DeriveAttack());
            }

            EnemyData.Add(importData.Id, newEnemy);
        }

        public static EnemyModel GetModel(string id)
        {
            if (!EnemyData.TryGetValue(id, out EnemyModel foundModel))
            {
                Debug.LogError($"EnemyData dictionary lookup does not contain id {id}");
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