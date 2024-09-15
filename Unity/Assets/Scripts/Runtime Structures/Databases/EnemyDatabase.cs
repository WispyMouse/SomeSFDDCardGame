namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
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

        public static EnemyModel GetModel(string id, RandomDecider<EnemyModel> decider = null)
        {
            if (decider == null)
            {
                decider = new RandomDecider<EnemyModel>();
            }

            string lowerId = id.ToLower();

            // If there are brackets, this might be a set of tag criteria.
            Match tagMatches = Regex.Match(id, @"\[([^]]+)\]");
            if (tagMatches.Success)
            {
                HashSet<string> tags = new HashSet<string>();
                foreach (Capture curCapture in tagMatches.Groups[1].Captures)
                {
                    tags.Add(curCapture.Value.ToLower());
                }

                if (!TryGetEnemyWithAllTags(decider, tags, out EnemyModel model))
                {
                    return null;
                }

                return model;
            }
            else
            {
                if (!EnemyData.TryGetValue(lowerId, out EnemyModel foundModel))
                {
                    Debug.LogError($"EnemyData dictionary lookup does not contain id {lowerId}");
                }
                return foundModel;
            }
        }

        public static void ClearDatabase()
        {
            EnemyData.Clear();
        }

        public static bool TryGetEnemyWithAllTags(RandomDecider<EnemyModel> decider, HashSet<string> tags, out EnemyModel enemy)
        {
            List<EnemyModel> candidates = new List<EnemyModel>();

            foreach (EnemyModel model in EnemyData.Values)
            {
                if (model.MeetsAllTags(tags))
                {
                    candidates.Add(model);
                }
            }

            if (candidates.Count == 0)
            {
                enemy = null;
                return false;
            }

            enemy = decider.ChooseRandomly(candidates);
            return true;
        }
    }
}