namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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

        public static Card GetModel(string id)
        {
            if (!CardData.TryGetValue(id, out Card foundModel))
            {
                Debug.LogError($"CardData dictionary lookup does not contain id {id}");
            }

            return foundModel;
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
    }
}