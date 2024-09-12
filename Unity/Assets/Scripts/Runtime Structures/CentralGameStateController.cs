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
        public Player CurrentPlayer { get; private set; } = null;

        public CampaignContext CurrentCampaignContext { get; private set; } = null;

        [SerializeReference]
        public GameplayUXController UXController;

        RunConfiguration CurrentRunConfiguration { get; set; } = null;

        private void Awake()
        {
        }

        void Start()
        {
            this.StartCoroutine(this.BootupSequence());
        }

        /// <summary>
        /// Starts up a new game and begins it.
        /// This will disable all other Controllers, reset all state based information, and generally clean the slate.
        /// Then the game will transition in to a new, playable state.
        /// </summary>
        public void SetupAndStartNewGame()
        {
            this.UXController.AddToLog("Resetting game to new state");

            this.UXController.Annihilate();

            this.CurrentCampaignContext = new CampaignContext();
            this.AssignStartingDeck();
            
            this.CurrentPlayer = new Player(this.CurrentRunConfiguration.StartingMaximumHealth);
            this.UXController.PlacePlayerCharacter();

            this.SetGameCampaignNavigationState(CampaignContext.GameplayCampaignState.ClearedRoom);
        }

        /// <summary>
        /// Leaves the current room, and loads in the next one, ready for gameplay.
        /// </summary>
        public void ProceedToNextRoom()
        {
            this.UXController.AddToLog("Proceeding to next room");

            this.CurrentCampaignContext.LeaveCurrentCombat();
            this.SetGameCampaignNavigationState(CampaignContext.GameplayCampaignState.EnteringRoom);

            Encounter newEncounter = EncounterDatabase.GetRandomEncounter();
            this.CurrentCampaignContext.StartNextRoomFromEncounter(newEncounter);

            if (newEncounter.IsShopEncounter)
            {
                List<Card> cardsToAward = CardDatabase.GetRandomCards(this.CurrentRunConfiguration.CardsInShop);
                this.UXController.ShowShopPanel(cardsToAward.ToArray());
                this.SetGameCampaignNavigationState(CampaignContext.GameplayCampaignState.NonCombatEncounter, CampaignContext.NonCombatEncounterStatus.AllowedToLeave);
            }
            else
            {
                this.SpawnEnemiesFromRoom();
                this.AssignEnemyIntents();
                this.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.DealCards(5);
                this.SetGameCampaignNavigationState(CampaignContext.GameplayCampaignState.InCombat);
            }
        }

        /// <summary>
        /// Creates the data structures for a deck, fills it with the starter cards, and sets that as the player's deck.
        /// </summary>
        void AssignStartingDeck()
        {
            foreach (string startingCard in this.CurrentRunConfiguration.StartingDeck)
            {
                this.CurrentCampaignContext.CampaignDeck.AddCardToDeck(CardDatabase.GetModel(startingCard).Clone());
            }
        }

        /// <summary>
        /// Sets up the current navigation state, and then reflects that on the UX.
        /// </summary>
        /// <param name="newState">The incoming state to configure for.</param>
        void SetGameCampaignNavigationState(CampaignContext.GameplayCampaignState newState, CampaignContext.NonCombatEncounterStatus noncombatState = CampaignContext.NonCombatEncounterStatus.NotInNonCombatEncounter)
        {
            this.CurrentCampaignContext.SetCampaignState(newState, noncombatState);

            // If the room is cleared, prepare to go to the next one by allowing for the button to be active.
            if (newState == CampaignContext.GameplayCampaignState.ClearedRoom)
            {
                this.CurrentCampaignContext.LeaveCurrentCombat();
                this.UXController.AddToLog($"Room is clear! Press Next Room to proceed to next encounter.");
            }

            if (newState == CampaignContext.GameplayCampaignState.InCombat)
            {
                this.UXController.AddToLog($"Combat start! Left click a card to select it, then left click an enemy to play it on them. Right click to deselect the currently selected card.");
                this.CurrentCampaignContext.CurrentCombatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
            }

            UpdateUXGlobalEvent.UpdateUXEvent?.Invoke();
        }

        void SpawnEnemiesFromRoom()
        {
            if (this.CurrentCampaignContext.CurrentCombatContext == null
                || this.CurrentCampaignContext.CurrentCombatContext.BasedOnEncounter == null)
            {
                Debug.LogException(new System.NullReferenceException($"The current room is null, and cannot have enemies added to it."));
                return;
            }

            foreach (string curEnemyId in this.CurrentCampaignContext.CurrentCombatContext.BasedOnEncounter.EnemiesInEncounterById)
            {
                EnemyModel curEnemyModel = EnemyDatabase.GetModel(curEnemyId);
                Enemy enemyInstance = new Enemy(curEnemyModel);
                this.CurrentCampaignContext.CurrentCombatContext.Enemies.Add(enemyInstance);

                this.UXController.AddEnemy(enemyInstance);
                this.UXController.AddToLog($"Enemy {enemyInstance.Name} spawned");
            }
        }

        /// <summary>
        /// Plays a specified card on the specified target.
        /// </summary>
        public void PlayCard(Card toPlay, ICombatantTarget toPlayOn)
        {
            if (this.UXController.PlayerIsCurrentlyAnimating)
            {
                this.UXController.AddToLog($"Player is currently animating, please wait until finished. (Being able to play faster will be fixed soon!)");
                return;
            }

            if (this.CurrentCampaignContext.CurrentCombatContext.CurrentTurnStatus != CombatContext.TurnStatus.PlayerTurn)
            {
                this.UXController.CancelAllSelections();
                return;
            }

            // Does the player meet the requirements of at least one of the effects?
            bool anyPassingRequirements = false;
            List<TokenEvaluatorBuilder> builders = ScriptTokenEvaluator.CalculateEvaluatorBuildersFromTokenEvaluation(toPlay);
            foreach (TokenEvaluatorBuilder builder in builders)
            {
                if (builder.MeetsElementRequirements(this.CurrentCampaignContext.CurrentCombatContext))
                {
                    anyPassingRequirements = true;
                    break;
                }
            }

            if (!anyPassingRequirements)
            {
                this.UXController.AddToLog($"Unable to play card {toPlay.Name}. No requirements for any of the card's effects have been met.");
                this.UXController.CancelAllSelections();
                return;
            }

            this.UXController.AddToLog($"Playing card {toPlay.Name} on {toPlayOn.Name}");
            this.UXController.CancelAllSelections();
            this.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand.Remove(toPlay);

            this.UXController.AnimateCardPlay(
                toPlay,
                toPlayOn,
                () =>
                {
                    GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(this, this.CurrentPlayer, toPlay, toPlayOn);
                    this.UXController.AddToLog(delta.DescribeDelta());
                    delta.ApplyDelta(this, this.UXController.AddToLog);
                    this.CheckAllStateEffectsAndKnockouts();
                },
                () =>
                {
                }
                );
        }

        void CheckAllStateEffectsAndKnockouts()
        {
            if (this.CurrentCampaignContext?.CurrentCombatContext?.Enemies != null)
            {
                List<Enemy> enemies = new List<Enemy>(this.CurrentCampaignContext.CurrentCombatContext.Enemies);
                foreach (Enemy curEnemy in enemies)
                {
                    if (curEnemy.ShouldBecomeDefeated)
                    {
                        this.UXController.AddToLog($"{curEnemy.Name} has been defeated!");
                        this.RemoveEnemy(curEnemy);
                    }
                }
            }

            if (this.CurrentPlayer.CurrentHealth <= 0)
            {
                this.UXController.AddToLog($"The player has run out of health! This run is over.");
                this.SetGameCampaignNavigationState(CampaignContext.GameplayCampaignState.Defeat);
                return;
            }

            if (this.CurrentCampaignContext.CurrentGameplayCampaignState == CampaignContext.GameplayCampaignState.NonCombatEncounter)
            {
                // 
            }
            else if (this.CurrentCampaignContext.CurrentCombatContext.Enemies.Count == 0)
            {
                this.UXController.AddToLog($"There are no more enemies!");
                this.SetupClearedRoomAndPresentAwards();
                return;
            }

            this.UXController.UpdateUX();
        }

        void SetupClearedRoomAndPresentAwards()
        {
            this.SetGameCampaignNavigationState(CampaignContext.GameplayCampaignState.ClearedRoom);
            List<Card> cardsToAward = CardDatabase.GetRandomCards(this.CurrentRunConfiguration.CardsToAwardOnVictory);
            this.UXController.ShowRewardsPanel(cardsToAward.ToArray());
        }

        void RemoveEnemy(Enemy toRemove)
        {
            this.UXController.RemoveEnemy(toRemove);
            this.CurrentCampaignContext.CurrentCombatContext.Enemies.Remove(toRemove);
        }

        public void EndTurn()
        {
            if (this.UXController.PlayerIsCurrentlyAnimating)
            {
                this.UXController.AddToLog($"Player is currently animating, please wait until finished. (Being able to play faster will be fixed soon!)");
                return;
            }

            if (this.CurrentCampaignContext.CurrentCombatContext.CurrentTurnStatus != CombatContext.TurnStatus.PlayerTurn)
            {
                return;
            }

            this.CurrentCampaignContext.CurrentCombatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.EnemyTurn);

            this.UXController.AnimateEnemyTurns(ContinueAfterEndTurnAnimationsFinished);
        }

        void ContinueAfterEndTurnAnimationsFinished()
        {
            this.CheckAllStateEffectsAndKnockouts();
            this.AssignEnemyIntents();
            this.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.DiscardHand();
            this.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.DealCards(5);

            this.CurrentCampaignContext.CurrentCombatContext.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn);
        }

        public void EnemyActsOnIntent(Enemy toAct)
        {
            GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(this, toAct, toAct.Intent, this.CurrentPlayer);
            this.UXController.AddToLog(delta.DescribeDelta());
            delta.ApplyDelta(this, this.UXController.AddToLog);
        }

        IEnumerator BootupSequence()
        {
            yield return LoadConfiguration();
            yield return LoadCards();
            yield return LoadStatusEffects();
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

            this.UXController.AddToLog($"Searched {cardImportPath}; Found {cardImportScriptNames.Length} scripts");

            foreach (string cardImportScriptName in cardImportScriptNames)
            {
                this.UXController.AddToLog($"Loading and parsing {cardImportScriptName}...");

                try
                {
                    string fileText = File.ReadAllText(cardImportScriptName);
                    CardImport importedCard = Newtonsoft.Json.JsonConvert.DeserializeObject<CardImport>(fileText);

                    string artLocation = $"{cardImportScriptName.Replace(".cardImport", ".png")}";
                    Sprite cardArt = null;
                    if (File.Exists(artLocation))
                    {
                        byte[] imageBytes = File.ReadAllBytes(artLocation);
                        Texture2D texture = new Texture2D(144, 80);
                        texture.LoadImage(imageBytes);
                        cardArt = Sprite.Create(texture, new Rect(0, 0, 144, 80), Vector2.zero);
                    }
                    else
                    {
                        this.UXController.AddToLog($"Could not find art for {cardImportScriptName} at expected location of {artLocation}");
                    }

                    CardDatabase.AddCardToDatabase(importedCard, cardArt);
                }
                catch (Exception e)
                {
                    this.UXController.AddToLog($"Failed to parse! Debug log has exception details.");
                    Debug.LogException(e);
                }
            }

            yield return null;
        }

        IEnumerator LoadStatusEffects()
        {
            string statusEffectImportPath = Application.streamingAssetsPath + "/statusImport";
            string[] statusEffectImportScriptNames = Directory.GetFiles(statusEffectImportPath, "*.statusImport");

            this.UXController.AddToLog($"Searched {statusEffectImportPath}; Found {statusEffectImportScriptNames.Length} scripts");

            foreach (string statusEffectImportScriptName in statusEffectImportScriptNames)
            {
                this.UXController.AddToLog($"Loading and parsing {statusEffectImportScriptName}...");

                try
                {
                    string fileText = File.ReadAllText(statusEffectImportScriptName);
                    StatusEffectImport importedStatusEffect = Newtonsoft.Json.JsonConvert.DeserializeObject<StatusEffectImport>(fileText);
                    StatusEffectDatabase.AddStatusEffectToDatabase(importedStatusEffect);
                }
                catch (Exception e)
                {
                    this.UXController.AddToLog($"Failed to parse! Debug log has exception details.");
                    Debug.LogException(e);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        IEnumerator LoadEnemyScripts()
        {
            string enemyImportPath = Application.streamingAssetsPath + "/enemyImport";
            string[] enemyImportScriptNames = Directory.GetFiles(enemyImportPath, "*.enemyimport");

            this.UXController.AddToLog($"Searched {enemyImportPath}; Found {enemyImportScriptNames.Length} scripts");

            foreach (string enemyImportScriptName in enemyImportScriptNames)
            {
                this.UXController.AddToLog($"Loading and parsing {enemyImportScriptName}...");

                try
                {
                    string fileText = File.ReadAllText(enemyImportScriptName);
                    EnemyImport importedEnemy = Newtonsoft.Json.JsonConvert.DeserializeObject<EnemyImport>(fileText);
                    EnemyDatabase.AddEnemyToDatabase(importedEnemy);
                }
                catch (Exception e)
                {
                    this.UXController.AddToLog($"Failed to parse! Debug log has exception details.");
                    Debug.LogException(e);
                }
            }

            string encounterImportPath = Application.streamingAssetsPath + "/encounterImport";
            string[] encounterImportNames = Directory.GetFiles(encounterImportPath, "*.encounterImport");

            this.UXController.AddToLog($"Searched {encounterImportPath}; Found {encounterImportNames.Length} scripts");

            foreach (string encounterImportScriptNames in encounterImportNames)
            {
                this.UXController.AddToLog($"Loading and parsing {encounterImportScriptNames}...");

                try
                {
                    string fileText = File.ReadAllText(encounterImportScriptNames);
                    EncounterImport importedEncounter = Newtonsoft.Json.JsonConvert.DeserializeObject<EncounterImport>(fileText);
                    EncounterDatabase.AddEncounter(importedEncounter);
                }
                catch (Exception e)
                {
                    this.UXController.AddToLog($"Failed to parse! Debug log has exception details.");
                    Debug.LogException(e);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        public void PlayerModelClicked()
        {
            this.UXController.SelectTarget(this.CurrentPlayer);
        }

        void AssignEnemyIntents()
        {
            foreach (Enemy curEnemy in this.CurrentCampaignContext.CurrentCombatContext.Enemies)
            {
                int randomAttackIndex = UnityEngine.Random.Range(0, curEnemy.BaseModel.Attacks.Count);
                EnemyAttack randomAttack = curEnemy.BaseModel.Attacks[randomAttackIndex];
                curEnemy.Intent = randomAttack;

                List<ICombatantTarget> consideredTargets = new List<ICombatantTarget>()
                {
                    curEnemy,
                    this.CurrentPlayer
                };

                List<ICombatantTarget> filteredTargets = ScriptTokenEvaluator.GetTargetsThatCanBeTargeted(curEnemy, curEnemy.Intent, consideredTargets);
                if (filteredTargets.Count == 0)
                {
                    randomAttack.PrecalculatedTarget = null;
                }
                else
                {
                    randomAttack.PrecalculatedTarget = filteredTargets[0];
                }
            }

            this.UXController.UpdateUX();
        }
    }
}
