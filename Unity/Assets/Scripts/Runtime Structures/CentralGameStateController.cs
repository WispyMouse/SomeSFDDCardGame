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

        [SerializeReference]
        private TextMeshProSpriteController TextMeshProSpriteControllerInstance;

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
            this.CurrentCampaignContext = null;
            this.UXController.ShowCampaignChooser();
        }

        IEnumerator BootupSequence()
        {
            yield return LoadConfiguration();
            yield return LoadElements();
            yield return LoadStatusEffects();
            yield return LoadCards();
            yield return LoadEnemyScripts();
            yield return LoadRoutes();
            this.SetupAndStartNewGame();
        }

        IEnumerator LoadConfiguration()
        {
            string configImportPath = Application.streamingAssetsPath;
            string fileText = File.ReadAllText(configImportPath + "/runconfiguration.runconfiguration");
            CurrentRunConfiguration = Newtonsoft.Json.JsonConvert.DeserializeObject<RunConfiguration>(fileText);
            yield return null;
        }

        IEnumerator LoadElements()
        {
            string elementImportPath = Application.streamingAssetsPath + "/elementImport";
            string[] elementImportScriptNames = Directory.GetFiles(elementImportPath, "*.elementimport", SearchOption.AllDirectories);

            GlobalUpdateUX.LogTextEvent.Invoke($"Searched {elementImportPath}; Found {elementImportScriptNames.Length} scripts", GlobalUpdateUX.LogType.Info);

            foreach (string elementImportScriptName in elementImportScriptNames)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Loading and parsing {elementImportScriptName}...", GlobalUpdateUX.LogType.Info);

                try
                {
                    string fileText = File.ReadAllText(elementImportScriptName);
                    ElementImport importedElement = Newtonsoft.Json.JsonConvert.DeserializeObject<ElementImport>(fileText);

                    if (importedElement == null)
                    {
                        GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse json at {elementImportScriptName}.", GlobalUpdateUX.LogType.RuntimeError);
                        continue;
                    }

                    string artLocation = $"{elementImportScriptName.ToLower().Replace(".elementimport", ".png")}";
                    Sprite elementArt = null;
                    if (File.Exists(artLocation))
                    {
                        byte[] imageBytes = File.ReadAllBytes(artLocation);
                        Texture2D texture = new Texture2D(64, 64);
                        texture.LoadImage(imageBytes);
                        elementArt = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.zero);
                    }
                    else
                    {
                        GlobalUpdateUX.LogTextEvent.Invoke($"Could not find art for {elementImportScriptName} at expected location of {artLocation}", GlobalUpdateUX.LogType.Info);
                    }

                    int? spriteIndex = null;
                    if (elementArt != null)
                    {
                        spriteIndex = this.TextMeshProSpriteControllerInstance.AddSprite(elementArt);
                    }

                    string greyscaleArtLocation = $"{elementImportScriptName.ToLower().Replace(".elementimport", ".greyscale.png")}";
                    Sprite greyscaleArt = null;
                    if (File.Exists(greyscaleArtLocation))
                    {
                        byte[] imageBytes = File.ReadAllBytes(greyscaleArtLocation);
                        Texture2D texture = new Texture2D(64, 64);
                        texture.LoadImage(imageBytes);
                        greyscaleArt = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.zero);
                    }
                    else
                    {
                        GlobalUpdateUX.LogTextEvent.Invoke($"Could not find art for {greyscaleArtLocation} at expected location of {artLocation}", GlobalUpdateUX.LogType.Info);
                    }

                    ElementDatabase.AddElement(importedElement, elementArt, greyscaleArt, spriteIndex);
                }
                catch (Exception e)
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse! Debug log has exception details.", GlobalUpdateUX.LogType.RuntimeError);
                    Debug.LogException(e);
                }
            }

            yield return null;
        }

        IEnumerator LoadCards()
        {
            string cardImportPath = Application.streamingAssetsPath + "/cardImport";
            string[] cardImportScriptNames = Directory.GetFiles(cardImportPath, "*.cardimport", SearchOption.AllDirectories);

            GlobalUpdateUX.LogTextEvent.Invoke($"Searched {cardImportPath}; Found {cardImportScriptNames.Length} scripts", GlobalUpdateUX.LogType.GameEvent);

            foreach (string cardImportScriptName in cardImportScriptNames)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Loading and parsing {cardImportScriptName}...", GlobalUpdateUX.LogType.GameEvent);

                try
                {
                    string fileText = File.ReadAllText(cardImportScriptName);
                    CardImport importedCard = Newtonsoft.Json.JsonConvert.DeserializeObject<CardImport>(fileText);

                    if (importedCard == null)
                    {
                        GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse json at {cardImportScriptName}.", GlobalUpdateUX.LogType.RuntimeError);
                        continue;
                    }

                    string artLocation = $"{cardImportScriptName.ToLower().Replace(".cardimport", ".png")}";
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
                        GlobalUpdateUX.LogTextEvent.Invoke($"Could not find art for {cardImportScriptName} at expected location of {artLocation}", GlobalUpdateUX.LogType.Info);
                    }

                    CardDatabase.AddCardToDatabase(importedCard, cardArt);
                }
                catch (Exception e)
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse! Debug log has exception details.", GlobalUpdateUX.LogType.RuntimeError);
                    Debug.LogException(e);
                }
            }

            yield return null;
        }

        IEnumerator LoadStatusEffects()
        {
            string statusEffectImportPath = Application.streamingAssetsPath + "/statusImport";
            string[] statusEffectImportScriptNames = Directory.GetFiles(statusEffectImportPath, "*.statusImport", SearchOption.AllDirectories);

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
            string[] enemyImportScriptNames = Directory.GetFiles(enemyImportPath, "*.enemyimport", SearchOption.AllDirectories);

            GlobalUpdateUX.LogTextEvent.Invoke($"Searched {enemyImportPath}; Found {enemyImportScriptNames.Length} scripts", GlobalUpdateUX.LogType.Info);

            foreach (string enemyImportScriptName in enemyImportScriptNames)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Loading and parsing {enemyImportScriptName}...", GlobalUpdateUX.LogType.Info);

                try
                {
                    string fileText = File.ReadAllText(enemyImportScriptName);
                    EnemyImport importedEnemy = Newtonsoft.Json.JsonConvert.DeserializeObject<EnemyImport>(fileText);
                    EnemyDatabase.AddEnemyToDatabase(importedEnemy);
                }
                catch (Exception e)
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse! Debug log has exception details.", GlobalUpdateUX.LogType.Info);
                    Debug.LogException(e);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        IEnumerator LoadRoutes()
        {
            string routeImportPath = Application.streamingAssetsPath + "/routeImport";
            string[] routeImportScriptNames = Directory.GetFiles(routeImportPath, "*.routeImport", SearchOption.AllDirectories);

            GlobalUpdateUX.LogTextEvent.Invoke($"Searched {routeImportPath}; Found {routeImportScriptNames.Length} scripts", GlobalUpdateUX.LogType.Info);

            string encounterImportPath = Application.streamingAssetsPath + "/encounterImport";
            string[] encounterImportNames = Directory.GetFiles(encounterImportPath, "*.encounterImport", SearchOption.AllDirectories);

            GlobalUpdateUX.LogTextEvent.Invoke($"Searched {encounterImportPath}; Found {encounterImportNames.Length} scripts", GlobalUpdateUX.LogType.Info);

            foreach (string encounterImportScriptNames in encounterImportNames)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Loading and parsing {encounterImportScriptNames}...", GlobalUpdateUX.LogType.Info);

                try
                {
                    string fileText = File.ReadAllText(encounterImportScriptNames);
                    EncounterImport importedEncounter = Newtonsoft.Json.JsonConvert.DeserializeObject<EncounterImport>(fileText);
                    EncounterDatabase.AddEncounter(importedEncounter);
                }
                catch (Exception e)
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse! Debug log has exception details.", GlobalUpdateUX.LogType.Info);
                    Debug.LogException(e);
                }
            }

            foreach (string routeImportScriptName in routeImportScriptNames)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Loading and parsing {routeImportScriptName}...", GlobalUpdateUX.LogType.Info);

                try
                {
                    string fileText = File.ReadAllText(routeImportScriptName);
                    RouteImport importedRoute = Newtonsoft.Json.JsonConvert.DeserializeObject<RouteImport>(fileText);
                    RouteDatabase.AddRouteToDatabase(importedRoute);
                }
                catch (Exception e)
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse! Debug log has exception details.", GlobalUpdateUX.LogType.Info);
                    Debug.LogException(e);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        public void RouteChosen(RouteImport route)
        {
            this.CurrentCampaignContext = new CampaignContext(this.CurrentRunConfiguration, this.UXController);
            this.CurrentCampaignContext.SetRoute(this.CurrentRunConfiguration, route);

            this.UXController.PlacePlayerCharacter();

            this.CurrentCampaignContext.SetCampaignState(CampaignContext.GameplayCampaignState.ClearedRoom);

            GlobalUpdateUX.UpdateUXEvent?.Invoke();
        }
    }
}
