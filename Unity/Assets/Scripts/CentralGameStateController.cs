namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
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
        public Dictionary<string, int> ElementResourceCounts { get; private set; } = null;

        RunConfiguration CurrentRunConfiguration { get; set; } = null;

        [SerializeReference]
        private PlayerUX PlayerRepresentationPF;
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
        private TMPro.TMP_Text ElementsValue;

        [SerializeReference]
        private TMPro.TMP_Text Log;

        private Dictionary<Enemy, EnemyUX> spawnedEnemiesLookup { get; set; } = new Dictionary<Enemy, EnemyUX>();

        public CardUX CurrentSelectedCard { get; private set; } = null;

        private void Awake()
        {
        }

        void Start()
        {
            this.StartCoroutine(this.BootupSequence());
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

            this.ElementResourceCounts = new Dictionary<string, int>();
            this.AssignStartingDeck();
            this.PlacePlayerCharacter();
            this.SetGameCampaignNavigationState(GameplayCampaignState.ClearedRoom);

            this.SetElementValueLabel();
        }

        /// <summary>
        /// Leaves the current room, and loads in the next one, ready for gameplay.
        /// </summary>
        public void ProceedToNextRoom()
        {
            this.AddToLog("Proceeding to next room");

            this.SetGameCampaignNavigationState(GameplayCampaignState.EnteringRoom);
            this.ElementResourceCounts.Clear();
            this.SpawnEnemiesFromRoom();
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

            foreach (string startingCard in this.CurrentRunConfiguration.StartingDeck)
            {
                this.CurrentDeck.AddCardToDeck(CardDatabase.GetModel(startingCard).Clone());
            }
        }

        /// <summary>
        /// Puts a graphical representation of the player character into the scene.
        /// </summary>
        void PlacePlayerCharacter()
        {
            PlayerUX playerObject = Instantiate(this.PlayerRepresentationPF, this.PlayerRepresentationTransform);

            playerObject.SetFromPlayer(this.CurrentPlayer, PlayerModelClicked);
            this.CurrentPlayer = new Player(50);

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
                this.CurrentRoom = new Room(EncounterDatabase.GetRandomEncounter());
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

        void SpawnEnemiesFromRoom()
        {
            for (int ii = this.EnemyRepresntationTransform.childCount - 1; ii >= 0; ii++)
            {
                Destroy(this.EnemyRepresntationTransform.GetChild(ii).gameObject);
            }

            if (this.CurrentRoom == null)
            {
                Debug.LogException(new System.NullReferenceException($"The current room is null, and cannot have enemies added to it."));
                return;
            }

            foreach (Enemy curEnemy in this.CurrentRoom.Enemies)
            {
                Vector3 objectOffset = new Vector3(3f, 0, 0) * this.EnemyRepresntationTransform.childCount;
                EnemyUX newEnemy = Instantiate(this.EnemyRepresentationPF, this.EnemyRepresntationTransform);
                newEnemy.transform.localPosition = objectOffset;
                newEnemy.SetFromEnemy(curEnemy, SelectTarget);
                this.spawnedEnemiesLookup.Add(curEnemy, newEnemy);

                this.AddToLog($"Enemy {newEnemy.RepresentedEnemy.Name} spawned");
            }
        }

        /// <summary>
        /// Plays a specified card on the specified target.
        /// </summary>
        public void PlayCard(Card toPlay, ICombatantTarget toPlayOn)
        {
            if (this.CurrentTurnStatus != TurnStatus.PlayerTurn)
            {
                return;
            }

            // Does the player meet the requirements of at least one of the effects?
            bool anyPassingRequirements = false;
            List<TokenEvaluatorBuilder> builders = ScriptTokenEvaluator.CalculateEvaluatorBuildersFromTokenEvaluation(this.CurrentPlayer, toPlay, toPlayOn);
            foreach (TokenEvaluatorBuilder builder in builders)
            {
                if (builder.MeetsElementRequirements(this))
                {
                    anyPassingRequirements = true;
                    break;
                }
            }

            if (!anyPassingRequirements)
            {
                this.AddToLog($"Unalbe to play card {toPlay.Name}. No requirements for any of the card's effects have been met.");
                this.CurrentSelectedCard?.DisableSelectionGlow();
                this.CurrentSelectedCard = null;
                return;
            }

            this.AddToLog($"Played card {toPlay.Name} on {toPlayOn.Name}");

            GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(this, this.CurrentPlayer, toPlay, toPlayOn);
            this.AddToLog(delta.DescribeDelta());
            delta.ApplyDelta(this, AddToLog);

            this.CheckAllStateEffectsAndKnockouts();
            this.CurrentDeck.CardsCurrentlyInHand.Remove(toPlay);
            this.RepresentPlayerHand();

            if (this.spawnedEnemiesLookup.Count == 0)
            {
                this.SetGameCampaignNavigationState(GameplayCampaignState.ClearedRoom);
            }
        }

        void CheckAllStateEffectsAndKnockouts()
        {
            this.SetElementValueLabel();

            List<Enemy> enemies = new List<Enemy>(this.CurrentRoom.Enemies);
            foreach (Enemy curEnemy in enemies)
            {
                if (curEnemy.ShouldBecomeDefeated)
                {
                    this.AddToLog($"{curEnemy.Name} has been defeated!");
                    this.RemoveEnemy(curEnemy);
                }
                else
                {
                    this.spawnedEnemiesLookup[curEnemy].UpdateUX();
                }
            }

            this.LifeValue.text = this.CurrentPlayer.CurrentHealth.ToString();
            this.RepresentPlayerHand();

            if (this.CurrentPlayer.CurrentHealth <= 0)
            {
                this.AddToLog($"The player has run out of health! This run is over.");
                this.SetGameCampaignNavigationState(GameplayCampaignState.Defeat);
                return;
            }
            if (this.CurrentRoom.Enemies.Count == 0)
            {
                this.AddToLog($"There are no more enemies!");
                this.SetGameCampaignNavigationState(GameplayCampaignState.ClearedRoom);
                return;
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

        public void SelectTarget(ICombatantTarget toSelect)
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
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            this.Log.text += "\n" + text;

            const int maximumLogSize = 10000;
            if (this.Log.text.Length > maximumLogSize)
            {
                this.Log.text = this.Log.text.Substring(this.Log.text.Length - maximumLogSize, maximumLogSize);
            }

            Debug.Log(text);
        }

        void RemoveEnemy(Enemy toRemove)
        {
            EnemyUX representation = this.spawnedEnemiesLookup[toRemove];
            Destroy(representation.gameObject);
            this.spawnedEnemiesLookup.Remove(toRemove);
            this.CurrentRoom.Enemies.Remove(toRemove);
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
                int randomAttackIndex = UnityEngine.Random.Range(0, curEnemy.BaseModel.Attacks.Count);
                EnemyAttack randomAttack = curEnemy.BaseModel.Attacks[randomAttackIndex];
                GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(this, curEnemy, randomAttack, this.CurrentPlayer);

                this.AddToLog(delta.DescribeDelta());

                delta.ApplyDelta(this, AddToLog);
            }

            this.CheckAllStateEffectsAndKnockouts();
            this.CurrentDeck.DiscardHand();
            this.CurrentDeck.DealCards(5);

            this.CurrentTurnStatus = TurnStatus.PlayerTurn;

            this.RepresentPlayerHand();
        }

        IEnumerator BootupSequence()
        {
            yield return LoadConfiguration();
            yield return LoadCards();
            yield return LoadEnemyScripts();
            this.SetupAndStartNewGame();
        }

        IEnumerator LoadConfiguration()
        {
            string configImportPath = Application.streamingAssetsPath;
            string fileText = File.ReadAllText(configImportPath + "/runconfiguration.runconfiguration");
            CurrentRunConfiguration = Newtonsoft.Json.JsonConvert.DeserializeObject<RunConfiguration>(fileText);
            yield return null;
        }

        IEnumerator LoadCards()
        {
            string cardImportPath = Application.streamingAssetsPath + "/cardImport";
            string[] cardImportScriptNames = Directory.GetFiles(cardImportPath, "*.cardimport");

            this.AddToLog($"Searched {cardImportPath}; Found {cardImportScriptNames.Length} scripts");

            foreach (string cardImportScriptName in cardImportScriptNames)
            {
                this.AddToLog($"Loading and parsing {cardImportScriptName}...");

                try
                {
                    string fileText = File.ReadAllText(cardImportScriptName);
                    CardImport importedCard = Newtonsoft.Json.JsonConvert.DeserializeObject<CardImport>(fileText);
                    CardDatabase.AddCardToDatabase(importedCard);
                }
                catch (Exception e)
                {
                    this.AddToLog($"Failed to parse! Debug log has exception details.");
                    Debug.LogException(e);
                }
            }

            yield return null;
        }

        IEnumerator LoadEnemyScripts()
        {
            string enemyImportPath = Application.streamingAssetsPath + "/enemyImport";
            string[] enemyImportScriptNames = Directory.GetFiles(enemyImportPath, "*.enemyimport");

            this.AddToLog($"Searched {enemyImportPath}; Found {enemyImportScriptNames.Length} scripts");

            foreach (string enemyImportScriptName in enemyImportScriptNames)
            {
                this.AddToLog($"Loading and parsing {enemyImportScriptName}...");

                try
                {
                    string fileText = File.ReadAllText(enemyImportScriptName);
                    EnemyImport importedEnemy = Newtonsoft.Json.JsonConvert.DeserializeObject<EnemyImport>(fileText);
                    EnemyDatabase.AddEnemyToDatabase(importedEnemy);
                }
                catch (Exception e)
                {
                    this.AddToLog($"Failed to parse! Debug log has exception details.");
                    Debug.LogException(e);
                }
            }

            string encounterImportPath = Application.streamingAssetsPath + "/encounterImport";
            string[] encounterImportNames = Directory.GetFiles(encounterImportPath, "*.encounterImport");

            this.AddToLog($"Searched {encounterImportPath}; Found {encounterImportNames.Length} scripts");

            foreach (string encounterImportScriptNames in encounterImportNames)
            {
                this.AddToLog($"Loading and parsing {encounterImportScriptNames}...");

                try
                {
                    string fileText = File.ReadAllText(encounterImportScriptNames);
                    EncounterImport importedEncounter = Newtonsoft.Json.JsonConvert.DeserializeObject<EncounterImport>(fileText);
                    EncounterDatabase.AddEncounter(importedEncounter);
                }
                catch (Exception e)
                {
                    this.AddToLog($"Failed to parse! Debug log has exception details.");
                    Debug.LogException(e);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        public void PlayerModelClicked()
        {
            this.SelectTarget(this.CurrentPlayer);
        }

        public void ApplyElementResourceChange(ElementResourceChange toChange)
        {
            if (this.ElementResourceCounts.TryGetValue(toChange.Element, out int currentAmount))
            {
                int newAmount = Mathf.Max(0, currentAmount + toChange.GainOrLoss);

                if (newAmount > 0)
                {
                    this.ElementResourceCounts[toChange.Element] = newAmount;
                }
                else
                {
                    this.ElementResourceCounts.Remove(toChange.Element);
                }
            }
            else
            {
                if (toChange.GainOrLoss > 0)
                {
                    this.ElementResourceCounts.Add(toChange.Element, toChange.GainOrLoss);
                }
            }

            this.SetElementValueLabel();
        }

        private void SetElementValueLabel()
        {
            if (this.ElementResourceCounts.Count == 0)
            {
                this.ElementsValue.text = "None";
                return;
            }

            string startingSeparator = "";
            StringBuilder compositeElements = new StringBuilder();
            foreach (string element in this.ElementResourceCounts.Keys)
            {
                compositeElements.Append($"{startingSeparator}{this.ElementResourceCounts[element]}\u00A0{element}");
                startingSeparator = ", ";
            }

            this.ElementsValue.text = compositeElements.ToString();
        }
    }
}
