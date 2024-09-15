namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class CardDatabase
    {
        public static Dictionary<string, Card> CardData { get; private set; } = new Dictionary<string, Card>();

        public static void AddCardToDatabase(CardImport importData, Sprite cardArt)
        {
            string lowerId = importData.Id.ToLower();

            if (CardData.ContainsKey(lowerId))
            {
                Debug.LogError($"CardData dictionary already contains id {lowerId}");
                return;
            }

            Card newCard = importData.DeriveCard();

            CardData.Add(lowerId, newCard);
            newCard.Sprite = cardArt;
        }

        public static Card GetModel(string id, RandomDecider<Card> decider = null)
        {
            if (decider == null)
            {
                decider = new RandomDecider<Card>();
            }

            // If there are brackets, this might be a set of tag criteria.
            Match tagMatches = Regex.Match(id, @"(?:\[(?<tag>[^]]+)\])+");
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
                if (!CardData.TryGetValue(id, out Card foundModel))
                {
                    Debug.LogError($"CardData dictionary lookup does not contain id {id}");
                }

                return foundModel;
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

        public static bool TryGetCardWithAllTags(RandomDecider<Card> decider, HashSet<string> tags, out Card card)
        {
            List<Card> candidates = new List<Card>();

            foreach (Card model in CardData.Values)
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

            card = decider.ChooseRandomly(candidates);
            return true;
        }
    }
}