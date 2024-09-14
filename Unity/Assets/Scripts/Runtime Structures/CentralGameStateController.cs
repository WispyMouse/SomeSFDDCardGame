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
        public CampaignContext CurrentCampaignContext { get; private set; } = null;

        [SerializeReference]
        private GameplayUXController UXController;

        [SerializeReference]
        private CombatTurnController CombatTurnControllerInstance;

        public RunConfiguration CurrentRunConfiguration { get; set; } = null;

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
            GlobalUpdateUX.LogTextEvent.Invoke("Resetting game to new state", GlobalUpdateUX.LogType.GameEvent);

            this.UXController.Annihilate();

            this.CurrentCampaignContext = new CampaignContext(this.CurrentRunConfiguration, this.UXController);
            
            this.UXController.PlacePlayerCharacter();

            this.SetGameCampaignNavigationState(CampaignContext.GameplayCampaignState.ClearedRoom);
        }

        /// <summary>
        /// Leaves the current room, and loads in the next one, ready for gameplay.
        /// </summary>
        public void ProceedToNextRoom()
        {
            GlobalUpdateUX.LogTextEvent.Invoke("Proceeding to next room", GlobalUpdateUX.LogType.GameEvent);

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
                this.CombatTurnControllerInstance.BeginHandlingCombat();
                this.SetGameCampaignNavigationState(CampaignContext.GameplayCampaignState.InCombat);
            }
        }

        /// <summary>
        /// Sets up the current navigation state, and then reflects that on the UX.
        /// </summary>
        /// <param name="newState">The incoming state to configure for.</param>
        public void SetGameCampaignNavigationState(CampaignContext.GameplayCampaignState newState, CampaignContext.NonCombatEncounterStatus noncombatState = CampaignContext.NonCombatEncounterStatus.NotInNonCombatEncounter)
        {
            this.CurrentCampaignContext.SetCampaignState(newState, noncombatState);

            // If the room is cleared, prepare to go to the next one by allowing for the button to be active.
            if (newState == CampaignContext.GameplayCampaignState.ClearedRoom)
            {
                this.CurrentCampaignContext.LeaveCurrentCombat();
                GlobalUpdateUX.LogTextEvent.Invoke($"Room is clear! Press Next Room to proceed to next encounter.", GlobalUpdateUX.LogType.GameEvent);
            }

            GlobalUpdateUX.UpdateUXEvent?.Invoke();
        }

        public void EnemyActsOnIntent(Enemy toAct)
        {
            GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(this.CurrentCampaignContext, toAct, toAct.Intent, this.CurrentCampaignContext.CampaignPlayer);
            GlobalUpdateUX.LogTextEvent.Invoke(delta.DescribeDelta(), GlobalUpdateUX.LogType.GameEvent);
            delta.ApplyDelta(this.CurrentCampaignContext);
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

            GlobalUpdateUX.LogTextEvent.Invoke($"Searched {cardImportPath}; Found {cardImportScriptNames.Length} scripts", GlobalUpdateUX.LogType.GameEvent);

            foreach (string cardImportScriptName in cardImportScriptNames)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Loading and parsing {cardImportScriptName}...", GlobalUpdateUX.LogType.GameEvent);

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
                        GlobalUpdateUX.LogTextEvent.Invoke($"Could not find art for {cardImportScriptName} at expected location of {artLocation}", GlobalUpdateUX.LogType.GameEvent);
                    }

                    CardDatabase.AddCardToDatabase(importedCard, cardArt);
                }
                catch (Exception e)
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse! Debug log has exception details.", GlobalUpdateUX.LogType.GameEvent);
                    Debug.LogException(e);
                }
            }

            yield return null;
        }

        IEnumerator LoadStatusEffects()
        {
            string statusEffectImportPath = Application.streamingAssetsPath + "/statusImport";
            string[] statusEffectImportScriptNames = Directory.GetFiles(statusEffectImportPath, "*.statusImport");

            GlobalUpdateUX.LogTextEvent.Invoke($"Searched {statusEffectImportPath}; Found {statusEffectImportScriptNames.Length} scripts", GlobalUpdateUX.LogType.GameEvent);

            foreach (string statusEffectImportScriptName in statusEffectImportScriptNames)
            {
                StatusEffectDatabase.TryImportStatusEffectFromFile(statusEffectImportScriptName, out _);
            }

            yield return new WaitForEndOfFrame();
        }

        IEnumerator LoadEnemyScripts()
        {
            string enemyImportPath = Application.streamingAssetsPath + "/enemyImport";
            string[] enemyImportScriptNames = Directory.GetFiles(enemyImportPath, "*.enemyimport");

            GlobalUpdateUX.LogTextEvent.Invoke($"Searched {enemyImportPath}; Found {enemyImportScriptNames.Length} scripts", GlobalUpdateUX.LogType.GameEvent);

            foreach (string enemyImportScriptName in enemyImportScriptNames)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Loading and parsing {enemyImportScriptName}...", GlobalUpdateUX.LogType.GameEvent);

                try
                {
                    string fileText = File.ReadAllText(enemyImportScriptName);
                    EnemyImport importedEnemy = Newtonsoft.Json.JsonConvert.DeserializeObject<EnemyImport>(fileText);
                    EnemyDatabase.AddEnemyToDatabase(importedEnemy);
                }
                catch (Exception e)
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse! Debug log has exception details.", GlobalUpdateUX.LogType.GameEvent);
                    Debug.LogException(e);
                }
            }

            string encounterImportPath = Application.streamingAssetsPath + "/encounterImport";
            string[] encounterImportNames = Directory.GetFiles(encounterImportPath, "*.encounterImport");

            GlobalUpdateUX.LogTextEvent.Invoke($"Searched {encounterImportPath}; Found {encounterImportNames.Length} scripts", GlobalUpdateUX.LogType.GameEvent);

            foreach (string encounterImportScriptNames in encounterImportNames)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Loading and parsing {encounterImportScriptNames}...", GlobalUpdateUX.LogType.GameEvent);

                try
                {
                    string fileText = File.ReadAllText(encounterImportScriptNames);
                    EncounterImport importedEncounter = Newtonsoft.Json.JsonConvert.DeserializeObject<EncounterImport>(fileText);
                    EncounterDatabase.AddEncounter(importedEncounter);
                }
                catch (Exception e)
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse! Debug log has exception details.", GlobalUpdateUX.LogType.GameEvent);
                    Debug.LogException(e);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        public void PlayerModelClicked()
        {
            this.UXController.SelectTarget(this.CurrentCampaignContext.CampaignPlayer);
        }
    }
}
