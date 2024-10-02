namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CombatDeck
    {
        public readonly Deck BasedOnDeck;

        /// <summary>
        /// Cards currently in the deck.
        /// This is an ordered list, with the card at the 0th index being the top of the deck.
        /// If a player would draw from an empty deck, they first shuffle their discard into their deck.
        /// </summary>
        public List<Card> CardsCurrentlyInDeck { get; private set; } = new List<Card>();

        /// <summary>
        /// A list of cards that are in the player's hand.
        /// </summary>
        public List<Card> CardsCurrentlyInHand { get; private set; } = new List<Card>();

        /// <summary>
        /// A list of cards that are in the player's discard.
        /// If a player would draw from an empty deck, they first shuffle their discard into their deck.
        /// </summary>
        public List<Card> CardsCurrentlyInDiscard { get; private set; } = new List<Card>();

        /// <summary>
        /// A list of cards that are in the player's exile.
        /// These cards are usually inaccessible. They do not get shuffled in like the discard.
        /// </summary>
        public List<Card> CardsCurrentlyInExile { get; private set; } = new List<Card>();

        public CombatDeck(Deck fromDeck)
        {
            this.BasedOnDeck = fromDeck;
        }

        /// <summary>
        /// Takes the cards in <see cref="AllCardsInDeck"/>, and then randomizes their order, and sets it to <see cref="CardsCurrentlyInDeck"/>.
        /// This should only be done at the start of a room, where you want a completely fresh deck state.
        /// </summary>
        public void ShuffleEntireDeck()
        {
            this.CardsCurrentlyInDeck = new List<Card>(this.BasedOnDeck.AllCardsInDeck).ShuffleList();
            this.CardsCurrentlyInDiscard.Clear();
            this.CardsCurrentlyInHand.Clear();
            this.CardsCurrentlyInExile.Clear();

            GlobalUpdateUX.UpdateUXEvent.Invoke();
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
                    this.MoveCardToZone(nextCard, this.CardsCurrentlyInHand);
                }
            }

            GlobalUpdateUX.UpdateUXEvent.Invoke();
        }

        public void ShuffleDiscardIntoDeck()
        {
            this.CardsCurrentlyInDeck.AddRange(this.CardsCurrentlyInDiscard);
            this.CardsCurrentlyInDiscard.Clear();
            this.CardsCurrentlyInDeck = this.CardsCurrentlyInDeck.ShuffleList();

            GlobalUpdateUX.UpdateUXEvent.Invoke();
        }

        public void DiscardHand()
        {
            for (int ii = this.CardsCurrentlyInHand.Count; ii > 0; ii--)
            {
                this.MoveCardToZone(this.CardsCurrentlyInHand[ii], this.CardsCurrentlyInDiscard);
            }

            GlobalUpdateUX.UpdateUXEvent.Invoke();
        }

        public void ShuffleDeck()
        {
            this.CardsCurrentlyInDeck = this.CardsCurrentlyInDeck.ShuffleList();
        }

        public void MoveCardToZone(Card card, List<Card> toMoveTo)
        {
            this.CardsCurrentlyInDeck.Remove(card);
            this.CardsCurrentlyInDiscard.Remove(card);
            this.CardsCurrentlyInHand.Remove(card);
            this.CardsCurrentlyInExile.Remove(card);

            toMoveTo.Add(card);

            GlobalUpdateUX.UpdateUXEvent.Invoke();
        }

        public void MoveCardToZoneIfNotInAnyZonesCurrently(Card card, List<Card> toMoveTo)
        {
            if (this.CardsCurrentlyInDiscard.Contains(card) || this.CardsCurrentlyInHand.Contains(card) || this.CardsCurrentlyInExile.Contains(card) || this.CardsCurrentlyInDeck.Contains(card))
            {
                return;
            }

            this.MoveCardToZone(card, toMoveTo);
        }
    }
}