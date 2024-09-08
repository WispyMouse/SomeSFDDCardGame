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
        public HashSet<Card> AllCardsInDeck { get; private set; } = new HashSet<Card>();

        /// <summary>
        /// Cards currently in the deck.
        /// This is an ordered list, with the card at the 0th index being the top of the deck.
        /// </summary>
        public List<Card> CardsCurrentlyInDeck { get; private set; } = new List<Card>();

        /// <summary>
        /// A list of cards that are in the player's hand.
        /// </summary>
        public List<Card> CardsCurrentlyInHand { get; private set; } = new List<Card>();

        /// <summary>
        /// A list of cards that are in the player's discard.
        /// </summary>
        public HashSet<Card> CardsCurrentlyInDiscard { get; private set; } = new HashSet<Card>();

        /// <summary>
        /// Adds a specified to card to the campaign level tracking of the deck, <see cref="AllCardsInDeck"/>.
        /// Does not add it to the <see cref="CardsCurrentlyInDeck"/>.
        /// </summary>
        /// <param name="toAdd"></param>
        public void AddCardToDeck(Card toAdd)
        {
            this.AllCardsInDeck.Add(toAdd);
        }

        /// <summary>
        /// Takes the cards in <see cref="AllCardsInDeck"/>, and then randomizes their order, and sets it to <see cref="CardsCurrentlyInDeck"/>.
        /// This should only be done at the start of a room, where you want a completely fresh deck state.
        /// </summary>
        public void ShuffleEntireDeck()
        {
            this.CardsCurrentlyInDeck = new List<Card>(this.AllCardsInDeck).ShuffleList();
            this.CardsCurrentlyInDiscard.Clear();
            this.CardsCurrentlyInHand.Clear();
        }

        /// <summary>
        /// Draws cards off the 'top' of the <see cref="CardsCurrentlyInDeck"/>.
        /// If there aren't enough cards in the deck, then cards in <see cref="CardsCurrentlyInDiscard"/> are shuffled into the deck.
        /// If there's still not enough cards, no card gets added to hand.
        /// </summary>
        /// <param name="numberOfCardsToDeal"></param>
        public void DealCards(int numberOfCardsToDeal)
        {
            for (int ii = 0; ii < numberOfCardsToDeal; ii++)
            {
                if (this.CardsCurrentlyInDeck.Count == 0)
                {
                    // Are there any cards in the discard?
                    // If not, then we can't possibly draw more cards
                    if (this.CardsCurrentlyInDiscard.Count == 0)
                    {
                        break;
                    }

                    this.ShuffleDiscardIntoDeck();
                }

                if (this.CardsCurrentlyInDeck.Count > 0)
                {
                    Card nextCard = this.CardsCurrentlyInDeck[0];
                    this.CardsCurrentlyInDeck.RemoveAt(0);
                    this.CardsCurrentlyInHand.Add(nextCard);
                }
            }
        }

        public void ShuffleDiscardIntoDeck()
        {
            this.CardsCurrentlyInDeck.AddRange(this.CardsCurrentlyInDiscard);
            this.CardsCurrentlyInDiscard.Clear();
            this.CardsCurrentlyInDeck = this.CardsCurrentlyInDeck.ShuffleList();
        }

        public void DiscardHand()
        {
            foreach (Card curCard in this.CardsCurrentlyInHand)
            {
                this.CardsCurrentlyInDiscard.Add(curCard);
            }

            this.CardsCurrentlyInHand.Clear();
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