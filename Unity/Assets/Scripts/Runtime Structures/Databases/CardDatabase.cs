namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class CardDatabase
    {
        public static Dictionary<string, CardImport> CardData { get; private set; } = new Dictionary<string, CardImport>();

        public static void AddCardToDatabase(CardImport importData)
        {
            string lowerId = importData.Id.ToLower();

            if (CardData.ContainsKey(lowerId))
            {
                Debug.LogError($"CardData dictionary already contains id {lowerId}");
                return;
            }

            CardData.Add(lowerId, importData);
        }

        public static Card GetModel(string id, RandomDecider<CardImport> decider = null)
        {
            if (decider == null)
            {
                decider = new RandomDecider<CardImport>();
            }

            string lowerId = id.ToLower().ToString();

            // If there are brackets, this might be a set of tag criteria.
            Match tagMatches = Regex.Match(lowerId, @"(?:\[(?<tag>[^]]+)\])+");
            if (tagMatches.Success)
            {
                HashSet<string> tags = new HashSet<string>();
                foreach (Capture curCapture in tagMatches.Groups[1].Captures)
                {
                    tags.Add(curCapture.Value.ToLower());
                }

                if (!TryGetCardWithAllTags(decider, tags, out Card model))
                {
                    return null;
                }

                return model;
            }
            else
            {
                if (!CardData.TryGetValue(lowerId, out CardImport foundModel))
                {
                    Debug.LogError($"CardData dictionary lookup does not contain id {lowerId}");
                }

                return new Card(foundModel);
            }
        }

        public static List<Card> GetRandomCards(int amount)
        {
            List<string> allCardCodes = new List<string>(CardDatabase.CardData.Keys);
            List<Card> toAward = new List<Card>();

            for (int ii = 0; ii < amount && allCardCodes.Count > 0; ii++)
            {
                int randomIndex = UnityEngine.Random.Range(0, allCardCodes.Count);
                string cardCode = allCardCodes[randomIndex];
                allCardCodes.RemoveAt(randomIndex);
                Card thisCard = CardDatabase.GetModel(cardCode);
                toAward.Add(thisCard);
            }

            return toAward;
        }

        public static bool TryGetCardWithAllTags(RandomDecider<CardImport> decider, HashSet<string> tags, out Card card)
        {
            List<CardImport> candidates = new List<CardImport>();

            foreach (CardImport model in CardData.Values)
            {
                if (model.MeetsAllTags(tags))
                {
                    candidates.Add(model);
                }
            }

            if (candidates.Count == 0)
            {
                card = null;
                return false;
            }

            card = new Card(decider.ChooseRandomly(candidates));
            return true;
        }

        public static Card[] GetOneOfEachCard()
        {
            List<Card> cards = new List<Card>();

            foreach (string id in CardData.Keys)
            {
                cards.Add(GetModel(id));
            }

            return cards.ToArray();
        }

        public static void ClearDatabase()
        {
            CardData.Clear();
        }
    }
}