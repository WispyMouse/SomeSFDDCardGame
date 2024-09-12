namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class Deck
    {
        /// <summary>
        /// Every card in the deck, regardless of its current location.
        /// This is effectively a campaign level tracking.
        /// </summary>
        public readonly HashSet<Card> AllCardsInDeck = new HashSet<Card>();

        /// <summary>
        /// Adds a specified to card to the campaign level tracking of the deck, <see cref="AllCardsInDeck"/>.
        /// Does not add it to the <see cref="CardsCurrentlyInDeck"/>.
        /// </summary>
        /// <param name="toAdd"></param>
        public void AddCardToDeck(Card toAdd)
        {
            this.AllCardsInDeck.Add(toAdd);
        }
    }

    public static class ListOfCardExtentions
    {
        public static List<Card> ShuffleList(this List<Card> listOfCards)
        {
            List<Card> workingCardList = new List<Card>(listOfCards);
            List<Card> resultingCardList = new List<Card>();

            while (workingCardList.Count > 0)
            {
                int randomIndex = Random.Range(0, workingCardList.Count);
                resultingCardList.Add(workingCardList[randomIndex]);
                workingCardList.RemoveAt(randomIndex);
            }

            return resultingCardList;
        }
    }
}