namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;


    public class CombatTurnController : MonoBehaviour
    {
        private static CombatTurnController Instance { get; set; }

        [SerializeReference]
        private CentralGameStateController CentralGameStateControllerInstance;

        [SerializeReference]
        private GameplayUXController UXController;

        private CombatContext Context => this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext;
        
        public bool CurrentlyActive { get; private set; } = false;
        public static readonly List<GameplaySequenceEvent> StackedSequenceEvents = new List<GameplaySequenceEvent>();

        private static Coroutine AnimationCoroutine { get; set; } = null;
        private static bool AnimationCoroutineIsRunning { get; set; } = false;
        private static GameplaySequenceEvent CurrentSequenceEvent { get; set; } = null;

        private void Awake()
        {
            if (Instance != null)
            {
                this.enabled = false;
                return;
            }

            Instance = this;
        }

        public void BeginHandlingCombat()
        {
            this.CurrentlyActive = true;

            PushSequencesToTop(
                new GameplaySequenceEvent(this.SpawnInitialEnemies, null),
                new GameplaySequenceEvent(() => this.Context.EndCurrentTurnAndChangeTurn(CombatContext.TurnStatus.PlayerTurn), null)
                );
        }

        public void EndHandlingCombat()
        {
            this.CurrentlyActive = false;
            StackedSequenceEvents.Clear();
        }

        #region Sequence Resolution

        private void Update()
        {
            do
            {
                if (StackedSequenceEvents.Count == 0 && CurrentSequenceEvent == null)
                {
                    return;
                }

                if (AnimationCoroutineIsRunning)
                {
                    return;
                }

                this.PopAndExecuteNextSequence();
            }
            while (StackedSequenceEvents.Count > 0);
        }

        private void PopAndExecuteNextSequence()
        {
            if (StackedSequenceEvents.Count == 0)
            {
                return;
            }

            CurrentSequenceEvent = StackedSequenceEvents[0];
            StackedSequenceEvents.RemoveAt(0);
            
            if (CurrentSequenceEvent.AnimationDelegate != null)
            {
                AnimationCoroutine = this.StartCoroutine(HandleSequenceEventWithAnimation(CurrentSequenceEvent));
            }
            else
            {
                CurrentSequenceEvent.ConsequentialAction?.Invoke();
                CurrentSequenceEvent = null;
            }
        }

        private IEnumerator HandleSequenceEventWithAnimation(GameplaySequenceEvent runningEvent)
        {
            AnimationCoroutineIsRunning = true;

            yield return runningEvent.AnimationDelegate();
            runningEvent.ConsequentialAction?.Invoke();

            AnimationCoroutineIsRunning = false;
            AnimationCoroutine = null;
        }

        #endregion

        #region Sequence Alteration

        /// <summary>
        /// Pushes a list of sequences to the top.
        /// The first element will be put in last, so it is
        /// the next thing that will happen.
        /// Used because it is intuitive to list things 
        /// in the order they happen while programming.
        /// </summary>
        public static void PushSequencesToTop(params GameplaySequenceEvent[] eventsToPush)
        {
            for (int ii = eventsToPush.Length - 1; ii >= 0; ii--)
            {
                PushSequenceToTop(eventsToPush[ii]);
            }
        }

        public static void PushSequenceToTop(GameplaySequenceEvent eventToPush)
        {
            if (eventToPush == null)
            {
                Debug.LogError($"{nameof(CombatTurnController)} ({nameof(PushSequenceToTop)}): Null event tried to push to top.");
            }

            StackedSequenceEvents.Insert(0, eventToPush);
        }

        public static void StopAllSequences()
        {
            if (AnimationCoroutine != null)
            {
                Instance.StopCoroutine(AnimationCoroutine);
                AnimationCoroutine = null;
            }

            StackedSequenceEvents.Clear();
            CurrentSequenceEvent = null;
        }

        #endregion

        #region Specific Gameplay Turn Concepts

        private void SpawnInitialEnemies()
        {
            foreach (Enemy curEnemy in this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.Enemies)
            {
                this.SpawnEnemy(curEnemy);
            }
        }

        private void SpawnEnemy(Enemy toSpawn)
        {
            this.UXController.AddEnemy(toSpawn);
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