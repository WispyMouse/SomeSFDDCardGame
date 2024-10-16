namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;


    public class CombatTurnController : MonoBehaviour, ICombatTurnController
    {
        private static CombatTurnController Instance { get; set; }

        [SerializeReference]
        private CentralGameStateController CentralGameStateControllerInstance;

        [SerializeReference]
        private GameplayUXController UXController;
        [SerializeReference]
        private EnemyRepresenterUX EnemyRepresenterUX;

        private CombatContext Context => this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext;
        
        public bool CurrentlyActive { get; private set; } = false;

        private static Coroutine AnimationCoroutine { get; set; } = null;
        private static bool AnimationCoroutineIsRunning { get; set; } = false;

        private void Awake()
        {
            if (Instance != null)
            {
                this.enabled = false;
                return;
            }

            Instance = this;

            GlobalSequenceEventHolder.OnStopAllSequences.AddListener(StopSequenceAnimation);
        }

        public void BeginHandlingCombat()
        {
            this.CurrentlyActive = true;

            GlobalSequenceEventHolder.PushSequencesToTop(
                CentralGameStateControllerInstance.CurrentCampaignContext,
                new GameplaySequenceEvent(this.SpawnInitialEnemies, null),
                new GameplaySequenceEvent(() => this.Context.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn), null)
                );
        }

        public void EndHandlingCombat()
        {
            this.CurrentlyActive = false;
            GlobalSequenceEventHolder.StopAllSequences();
        }

        private void StopSequenceAnimation()
        {
            if (AnimationCoroutine != null)
            {
                Instance.StopCoroutine(AnimationCoroutine);
                AnimationCoroutine = null;
            }
        }

        #region Sequence Resolution

        private void Update()
        {
            do
            {
                if (GlobalSequenceEventHolder.StackedSequenceEvents.Count == 0 && GlobalSequenceEventHolder.CurrentSequenceEvent == null)
                {
                    return;
                }

                if (AnimationCoroutineIsRunning)
                {
                    return;
                }

                GlobalSequenceEventHolder.ApplyNextSequenceWithAnimationHandler(this);
            }
            while (GlobalSequenceEventHolder.StackedSequenceEvents.Count > 0);
        }

        public void HandleSequenceEventWithAnimation(GameplaySequenceEvent sequenceEvent)
        {
            AnimationCoroutine = this.StartCoroutine(AnimateHandleSequenceEventWithAnimation(sequenceEvent));
        }

        private IEnumerator AnimateHandleSequenceEventWithAnimation(GameplaySequenceEvent runningEvent)
        {
            AnimationCoroutineIsRunning = true;

            yield return runningEvent.AnimationDelegate();
            runningEvent.ConsequentialAction?.Invoke();

            GlobalUpdateUX.UpdateUXEvent.Invoke();

            AnimationCoroutineIsRunning = false;
            AnimationCoroutine = null;
        }

        #endregion

        #region Specific Gameplay Turn Concepts

        private void SpawnInitialEnemies()
        {
            this.EnemyRepresenterUX.AddEnemies(this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.Enemies);
        }

        private void SpawnEnemy(Enemy toSpawn)
        {
            this.EnemyRepresenterUX.AddEnemy(toSpawn);
        }

        public void EndPlayerTurn()
        {
            this.Context.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.EnemyTurn);
        }

        public void PlayCard(Card toPlay, ICombatantTarget toPlayOn)
        {
            this.Context.PlayCard(toPlay, toPlayOn);
        }

        #endregion
    }
}