namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class EncounterDatabase
    {
        public static Dictionary<string, EncounterModel> EncounterData { get; set; } = new Dictionary<string, EncounterModel>();

        public static void AddEncounter(EncounterImport toAdd)
        {
            EncounterData.Add(toAdd.Id, new EncounterModel(toAdd));
        }

        public static bool TryGetEncounterWithArguments(RandomDecider<EncounterModel> decider, string kind, List<string> arguments, out EvaluatedEncounter encounter)
        {
            kind = kind.ToLower();

            // If there are brackets, this might be a set of tag criteria.
            Match tagMatches = Regex.Match(kind, @"\[([^]]+)\]");
            if (tagMatches.Success)
            {
                HashSet<string> tags = new HashSet<string>();
                foreach (Capture curCapture in tagMatches.Groups[1].Captures)
                {
                    tags.Add(curCapture.Value.ToLower());
                }
                
                if (!TryGetEncounterWithAllTags(decider, tags, out EncounterModel model))
                {
                    encounter = null;
                    return false;
                }

                encounter = GetEvaluatorForKind(model);
                return true;
            }
            else
            {
                if (!EncounterData.TryGetValue(kind, out EncounterModel encounterModel))
                {
                    encounter = null;
                    return false;
                }

                encounter = GetEvaluatorForKind(encounterModel);
                return true;
            }
        }

        public static bool TryGetEncounterWithAllTags(RandomDecider<EncounterModel> decider, HashSet<string> tags, out EncounterModel encounter)
        {
            List<EncounterModel> candidates = new List<EncounterModel>();

            foreach (EncounterModel model in EncounterData.Values)
            {
                if (model.MeetsAllTags(tags))
                {
                    candidates.Add(model);
                }
            }

            if (candidates.Count == 0)
            {
                encounter = null;
                return false;
            }

            encounter = decider.ChooseRandomly(candidates);
            return true;
        }

        public static EvaluatedEncounter GetEvaluatorForKind(EncounterModel model)
        {
            return new EvaluatedEncounter(model);
        }
    }
}