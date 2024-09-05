namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CentralGameStateController : MonoBehaviour
    {
        public enum GameplayCampaignState
        {
            NotStarted = 0,
            ClearedRoom = 1,
            InCombat = 2,
            Defeat = 3,
            EnteringRoom = 4
        }

        public GameplayCampaignState CurrentGameplayCampaignState { get; private set; } = GameplayCampaignState.NotStarted;
        public Room CurrentRoom { get; private set; } = null;
        public Deck CurrentDeck { get; private set; } = null;

        [SerializeReference]
        private GameObject PlayerRepresentationPF;
        [SerializeReference]
        private EnemyUX EnemyRepresentationPF;
        [SerializeReference]
        private CardUX CardRepresentationPF;
        [SerializeReference]
        private Transform PlayerHandTransform;
        [SerializeReference]
        private Transform PlayerRepresentationTransform;
        [SerializeReference]
        private Transform EnemyRepresntationTransform;

        [SerializeReference]
        private GameObject GoNextRoomButton;
        [SerializeReference]
        private GameObject ResetGameButton;

        [SerializeReference]
        private TMPro.TMP_Text Log;

        public Card CurrentSelectedCard { get; private set; } = null;

        void Start()
        {
            this.SetupAndStartNewGame();
        }

        /// <summary>
        /// Starts up a new game and begins it.
        /// This will disable all other Controllers, reset all state based information, and generally clean the slate.
        /// Then the game will transition in to a new, playable state.
        /// </summary>
        public void SetupAndStartNewGame()
        {
            this.AddToLog("Resetting game to new state");

            for (int ii = this.PlayerHandTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.PlayerHandTransform.GetChild(ii).gameObject);
            }

            for (int ii = this.PlayerRepresentationTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.PlayerRepresentationTransform.GetChild(ii).gameObject);
            }

            for (int ii = this.EnemyRepresntationTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.EnemyRepresntationTransform.GetChild(ii).gameObject);
            }

            this.AssignStartingDeck();
            this.PlacePlayerCharacter();
            this.SetGameCampaignNavigationState(GameplayCampaignState.ClearedRoom);
        }

        /// <summary>
        /// Leaves the current room, and loads in the next one, ready for gameplay.
        /// </summary>
        public void ProceedToNextRoom()
        {
            this.AddToLog("Proceeding to next room");

            this.SetGameCampaignNavigationState(GameplayCampaignState.EnteringRoom);
            this.SpawnEnemy();
            this.CurrentDeck.ShuffleEntireDeck();
            this.CurrentDeck.DealCards(5);
            this.RepresentPlayerHand();
            this.SetGameCampaignNavigationState(GameplayCampaignState.InCombat);
        }

        /// <summary>
        /// Creates the data structures for a deck, fills it with the starter cards, and sets that as the player's deck.
        /// </summary>
        void AssignStartingDeck()
        {
            this.CurrentDeck = new Deck();

            const int NumberOfCardsInStarterDeck = 20;

            for (int ii = 0; ii < NumberOfCardsInStarterDeck; ii++)
            {
                this.CurrentDeck.AddCardToDeck(new Card());
            }
        }

        /// <summary>
        /// Puts a graphical representation of the player character into the scene.
        /// </summary>
        void PlacePlayerCharacter()
        {
            GameObject playerObject = Instantiate(this.PlayerRepresentationPF, this.PlayerRepresentationTransform);
        }

        /// <summary>
        /// Sets up the current navigation state, and then reflects that on the UX.
        /// </summary>
        /// <param name="newState">The incoming state to configure for.</param>
        void SetGameCampaignNavigationState(GameplayCampaignState newState)
        {
            this.CurrentGameplayCampaignState = newState;

            // If the room is cleared, prepare to go to the next one by allowing for the button to be active.
            if (newState == GameplayCampaignState.ClearedRoom)
            {
                this.GoNextRoomButton.SetActive(true);
            }
            else
            {
                this.GoNextRoomButton.SetActive(false);
            }

            if (newState == GameplayCampaignState.EnteringRoom)
            {
                this.CurrentRoom = new Room();

                for (int ii = this.EnemyRepresntationTransform.childCount - 1; ii >= 0; ii++)
                {
                    Destroy(this.EnemyRepresntationTransform.GetChild(ii).gameObject);
                }
            }
        }

        /// <summary>
        /// Adds an enemy to the current room, and spawns them in visually.
        /// </summary>
        void SpawnEnemy()
        {
            if (this.CurrentRoom == null)
            {
                Debug.LogException(new System.NullReferenceException($"The current room is null, and cannot have enemies added to it."));
                return;
            }

            Enemy currentEnemy = new Enemy();
            this.CurrentRoom.AddEnemy(currentEnemy);
            Vector3 objectOffset = new Vector3(50f, 0, 0) * this.EnemyRepresntationTransform.childCount;
            EnemyUX newEnemy = Instantiate(this.EnemyRepresentationPF, this.EnemyRepresntationTransform);
            newEnemy.transform.localPosition = objectOffset;
            newEnemy.SetFromEnemy(currentEnemy, SelectEnemy);

            AddToLog($"Enemy {newEnemy} spawned");
        }

        /// <summary>
        /// Plays a specified card on the specified target.
        /// </summary>
        public void PlayCard(Card toPlay, Enemy toPlayOn)
        {
            AddToLog($"Played card {toPlay} on {toPlayOn}");
            this.CurrentDeck.CardsCurrentlyInHand.Remove(toPlay);
            this.RepresentPlayerHand();
        }

        /// <summary>
        /// Takes the current player hand, makes the appropriate cards, and sets them in the player's hand.
        /// </summary>
        void RepresentPlayerHand()
        {
            for (int ii = this.PlayerHandTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.PlayerHandTransform.GetChild(ii).gameObject);
            }

            for (int ii = 0; ii < this.CurrentDeck.CardsCurrentlyInHand.Count; ii++)
            {
                Vector3 objectOffset = new Vector3(.5f, 0, 0) * ii;
                CardUX newCard = Instantiate(this.CardRepresentationPF, this.PlayerHandTransform);
                newCard.transform.localPosition = objectOffset;
                newCard.SetFromCard(this.CurrentDeck.CardsCurrentlyInHand[ii], SelectCurrentCard);
            }
        }

        public void SelectCurrentCard(Card toSelect)
        {
            this.CurrentSelectedCard = toSelect;
        }

        public void SelectEnemy(Enemy toSelect)
        {
            if (this.CurrentSelectedCard == null)
            {
                return;
            }

            this.PlayCard(this.CurrentSelectedCard, toSelect);
            this.CurrentSelectedCard = null;
        }

        public void AddToLog(string text)
        {
            this.Log.text += "\n" + text;

            const int maximumLogSize = 10000;
            if (this.Log.text.Length > maximumLogSize)
            {
                this.Log.text = this.Log.text.Substring(this.Log.text.Length - maximumLogSize, maximumLogSize);
            }
        }
    }
}
