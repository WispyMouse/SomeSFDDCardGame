namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class CardDatabase
    {
        public static Dictionary<string, Card> CardData { get; private set; } = new Dictionary<string, Card>();

        public static void AddCardToDatabase(CardImport importData)
        {
            if (CardData.ContainsKey(importData.Id))
            {
                Debug.LogError($"CardData dictionary already contains id {importData.Id}");
                return;
            }

            Card newCard = importData.DeriveCard();
            CardData.Add(importData.Id, newCard);
        }

        public static Card GetModel(string id)
        {
            if (!CardData.TryGetValue(id, out Card foundModel))
            {
                Debug.LogError($"CardData dictionary lookup does not contain id {id}");
            }

            return foundModel;
        }
    }
}