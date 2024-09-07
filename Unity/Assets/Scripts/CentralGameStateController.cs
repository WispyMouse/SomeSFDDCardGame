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

        public enum TurnStatus
        {
            NotInCombat = 0,
            PlayerTurn = 1,
            EnemyTurn = 2
        }

        public GameplayCampaignState CurrentGameplayCampaignState { get; private set; } = GameplayCampaignState.NotStarted;
        public TurnStatus CurrentTurnStatus { get; private set; } = TurnStatus.NotInCombat;

        public Room CurrentRoom { get; private set; } = null;
        public Deck CurrentDeck { get; private set; } = null;
        public Player CurrentPlayer { get; private set; } = null;

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
        private GameObject EndTurnButton;

        [SerializeReference]
        private GameObject DeckStatPanel;
        [SerializeReference]
        private TMPro.TMP_Text CardsInDeckValue;
        [SerializeReference]
        private TMPro.TMP_Text CardsInDiscardValue;
        [SerializeReference]
        private TMPro.TMP_Text LifeValue;

        [SerializeReference]
        private TMPro.TMP_Text Log;

        private Dictionary<Enemy, EnemyUX> spawnedEnemiesLookup { get; set; } = new Dictionary<Enemy, EnemyUX>();

        public CardUX CurrentSelectedCard { get; private set; } = null;

        void Start()
        {
            this.SetupAndStartNewGame();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(2))
            {
                if (this.CurrentSelectedCard != null)
                {
                    this.CurrentSelectedCard.DisableSelectionGlow();
                    this.CurrentSelectedCard = null;
                }
            }
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
                this.CurrentDeck.AddCardToDeck(
                    new Card()
                    {
                        Name = "Simple Attack",
                        EffectText = "Deals 1 damage to any enemy"
                    }
                    );
            }
        }

        /// <summary>
        /// Puts a graphical representation of the player character into the scene.
        /// </summary>
        void PlacePlayerCharacter()
        {
            GameObject playerObject = Instantiate(this.PlayerRepresentationPF, this.PlayerRepresentationTransform);

            this.CurrentPlayer = new Player()
            {
                MaxHealth = 50,
                CurrentHealth = 50
            };

            this.LifeValue.text = $"{this.CurrentPlayer.CurrentHealth} / {this.CurrentPlayer.MaxHealth}";
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
                this.CurrentTurnStatus = TurnStatus.NotInCombat;
                this.GoNextRoomButton.SetActive(true);
                this.AddToLog($"Room is clear! Press Next Room to proceed to next encounter.");
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

            if (newState == GameplayCampaignState.InCombat)
            {
                this.AddToLog($"Combat start! Left click a card to select it, then left click an enemy to play it on them. Right click to deselect the currently selected card.");
                this.CurrentTurnStatus = TurnStatus.PlayerTurn;
                this.EndTurnButton.SetActive(true);
            }
            else
            {
                this.EndTurnButton.SetActive(false);
            }

            if (newState == GameplayCampaignState.Defeat)
            {

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

            Enemy currentEnemy = new Enemy()
            {
                Name = "Some Shmuck",
                MaximumHealth = 3,
                CurrentHealth = 3
            };

            this.CurrentRoom.AddEnemy(currentEnemy);
            Vector3 objectOffset = new Vector3(50f, 0, 0) * this.EnemyRepresntationTransform.childCount;
            EnemyUX newEnemy = Instantiate(this.EnemyRepresentationPF, this.EnemyRepresntationTransform);
            newEnemy.transform.localPosition = objectOffset;
            newEnemy.SetFromEnemy(currentEnemy, SelectEnemy);
            this.spawnedEnemiesLookup.Add(currentEnemy, newEnemy);

            this.AddToLog($"Enemy {newEnemy} spawned");
        }

        /// <summary>
        /// Plays a specified card on the specified target.
        /// </summary>
        public void PlayCard(Card toPlay, Enemy toPlayOn)
        {
            if (this.CurrentTurnStatus != TurnStatus.PlayerTurn)
            {
                return;
            }

            this.AddToLog($"Played card {toPlay.Name} on {toPlayOn.Name}");

            int newEnemyHealth = Mathf.Max(0, toPlayOn.CurrentHealth - 1);
            this.AddToLog($"Dealing 1 damage to {toPlayOn.Name}. {toPlayOn.CurrentHealth} => {newEnemyHealth}");
            toPlayOn.CurrentHealth = newEnemyHealth;

            if (toPlayOn.ShouldBecomeDefeated)
            {
                this.AddToLog($"{toPlayOn.Name} has been defeated!");
                this.RemoveEnemy(toPlayOn);
            }
            else
            {
                this.spawnedEnemiesLookup[toPlayOn].UpdateUX();
            }

            this.CurrentDeck.CardsCurrentlyInHand.Remove(toPlay);
            this.RepresentPlayerHand();

            if (this.spawnedEnemiesLookup.Count == 0)
            {
                this.SetGameCampaignNavigationState(GameplayCampaignState.ClearedRoom);
            }
        }

        /// <summary>
        /// Takes the current player hand, makes the appropriate cards, and sets them in the player's hand.
        /// </summary>
        void RepresentPlayerHand()
        {
            const float CardFanDistance = 2f;

            for (int ii = this.PlayerHandTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.PlayerHandTransform.GetChild(ii).gameObject);
            }

            float leftStartingPoint = -CardFanDistance * (this.CurrentDeck.CardsCurrentlyInHand.Count - 1) / 2f;

            for (int ii = 0; ii < this.CurrentDeck.CardsCurrentlyInHand.Count; ii++)
            {
                Vector3 objectOffset = new Vector3(leftStartingPoint, 0, 0) + new Vector3(CardFanDistance, 0, 0) * ii;
                CardUX newCard = Instantiate(this.CardRepresentationPF, this.PlayerHandTransform);
                newCard.transform.localPosition = objectOffset;
                newCard.SetFromCard(this.CurrentDeck.CardsCurrentlyInHand[ii], SelectCurrentCard);
            }

            this.CardsInDeckValue.text = this.CurrentDeck.CardsCurrentlyInDeck.Count.ToString();
            this.CardsInDiscardValue.text = this.CurrentDeck.CardsCurrentlyInDiscard.Count.ToString();
        }

        public void SelectCurrentCard(CardUX toSelect)
        {
            if (this.CurrentTurnStatus != TurnStatus.PlayerTurn)
            {
                return;
            }

            if (this.CurrentSelectedCard != null)
            {
                this.CurrentSelectedCard.DisableSelectionGlow();
            }

            this.CurrentSelectedCard = toSelect;
            this.CurrentSelectedCard.EnableSelectionGlow();
            AddToLog($"Selected card {toSelect.RepresentedCard.Name}");
        }

        public void SelectEnemy(Enemy toSelect)
        {
            if (this.CurrentSelectedCard == null)
            {
                return;
            }

            if (this.CurrentTurnStatus != TurnStatus.PlayerTurn)
            {
                return;
            }

            this.PlayCard(this.CurrentSelectedCard.RepresentedCard, toSelect);
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

        void RemoveEnemy(Enemy toRemove)
        {
            EnemyUX representation = this.spawnedEnemiesLookup[toRemove];
            Destroy(representation.gameObject);
            this.spawnedEnemiesLookup.Remove(toRemove);
        }

        public void EndTurn()
        {
            if (this.CurrentTurnStatus != TurnStatus.PlayerTurn)
            {
                return;
            }

            this.CurrentTurnStatus = TurnStatus.EnemyTurn;

            foreach (Enemy curEnemy in this.spawnedEnemiesLookup.Keys)
            {
                int newPlayerHealth = Mathf.Max(0, this.CurrentPlayer.CurrentHealth - 1);
                this.AddToLog($"{curEnemy.Name} deals damage to the player. {this.CurrentPlayer.CurrentHealth} => {newPlayerHealth}");
                this.CurrentPlayer.CurrentHealth = newPlayerHealth;
                this.LifeValue.text = $"{this.CurrentPlayer.CurrentHealth} / {this.CurrentPlayer.MaxHealth}";
            }

            if (this.CurrentPlayer.CurrentHealth <= 0)
            {
                this.AddToLog($"The player has run out of health! This run is over.");
                this.SetGameCampaignNavigationState(GameplayCampaignState.Defeat);
                return;
            }

            this.CurrentDeck.DiscardHand();
            this.CurrentDeck.DealCards(5);

            this.CurrentTurnStatus = TurnStatus.PlayerTurn;

            this.RepresentPlayerHand();
        }
    }
}
