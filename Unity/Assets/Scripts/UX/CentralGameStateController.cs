namespace SFDDCards.UX
{
    using SFDDCards.ImportModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
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
            GlobalUpdateUX.UpdateUXEvent.Invoke();
        }

        IEnumerator BootupSequence()
        {
            yield return ImportHelper.YieldForTask(ImportHelper.ImportImportableFilesIntoDatabaseAsync<ElementImport>(Application.streamingAssetsPath, "elementImport", ElementDatabase.AddElement));
            foreach (Element element in ElementDatabase.ElementData.Values)
            {
                if (element.Sprite != null)
                {
                    int spriteIndex = this.TextMeshProSpriteControllerInstance.AddSprite(element.Sprite);
                    element.SpriteIndex = spriteIndex;
                }
            }

            yield return ImportHelper.YieldForTask(ImportHelper.ImportImportableFilesIntoDatabaseAsync<CardImport>(Application.streamingAssetsPath, "cardImport", CardDatabase.AddCardToDatabase));
            yield return ImportHelper.YieldForTask(ImportHelper.ImportImportableFilesIntoDatabaseAsync<StatusEffectImport>(Application.streamingAssetsPath, "statusImport", StatusEffectDatabase.AddStatusEffectToDatabase));
            yield return ImportHelper.YieldForTask(ImportHelper.ImportImportableFilesIntoDatabaseAsync<EnemyImport>(Application.streamingAssetsPath, "enemyImport", EnemyDatabase.AddEnemyToDatabase));

            yield return ImportHelper.YieldForTask(ImportHelper.ImportImportableFilesIntoDatabaseAsync<RewardImport>(Application.streamingAssetsPath, "rewardImport", RewardDatabase.AddRewardToDatabase));
            yield return ImportHelper.YieldForTask(ImportHelper.ImportImportableFilesIntoDatabaseAsync<EncounterImport>(Application.streamingAssetsPath, "encounterImport", EncounterDatabase.AddEncounter));
            yield return ImportHelper.YieldForTask(ImportHelper.ImportImportableFilesIntoDatabaseAsync<RouteImport>(Application.streamingAssetsPath, "routeImport", RouteDatabase.AddRouteToDatabase));

            Task<RunConfiguration> fileTask  = ImportHelper.GetFileAsync<RunConfiguration>(Application.streamingAssetsPath + "/runconfiguration.runconfiguration");
            yield return ImportHelper.YieldForTask(fileTask);
            CurrentRunConfiguration = fileTask.Result;


            this.SetupAndStartNewGame();
        }

        public void RouteChosen(RouteImport route)
        {
            this.CurrentCampaignContext = new CampaignContext(new CampaignRoute(this.CurrentRunConfiguration, route));

            this.UXController.PlacePlayerCharacter();

            this.CurrentCampaignContext.SetCampaignState(CampaignContext.GameplayCampaignState.MakingRouteChoice);

            GlobalUpdateUX.UpdateUXEvent?.Invoke();
        }
    }
}
