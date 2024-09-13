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
        [SerializeReference]
        private CentralGameStateController CentralGameStateControllerInstance;

        [SerializeReference]
        private GameplayUXController UXController;

        private CombatContext Context => this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext;
        
        public bool CurrentlyActive { get; private set; } = false;
        public static readonly List<GameplaySequenceEvent> StackedSequenceEvents = new List<GameplaySequenceEvent>();

        private Coroutine AnimationCoroutine { get; set; } = null;
        private bool AnimationCoroutineIsRunning { get; set; } = false;
        private GameplaySequenceEvent CurrentSequenceEvent { get; set; } = null;

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
                if (StackedSequenceEvents.Count == 0 && this.CurrentSequenceEvent == null)
                {
                    return;
                }

                if (this.AnimationCoroutineIsRunning)
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

            this.CurrentSequenceEvent = StackedSequenceEvents[0];
            StackedSequenceEvents.RemoveAt(0);
            
            if (this.CurrentSequenceEvent.AnimationDelegate != null)
            {
                this.AnimationCoroutine = this.StartCoroutine(HandleSequenceEventWithAnimation(this.CurrentSequenceEvent));
            }
            else
            {
                this.CurrentSequenceEvent.ConsequentialAction?.Invoke();
                this.CurrentSequenceEvent = null;
            }
        }

        private IEnumerator HandleSequenceEventWithAnimation(GameplaySequenceEvent runningEvent)
        {
            this.AnimationCoroutineIsRunning = true;

            yield return runningEvent.AnimationDelegate();
            runningEvent.ConsequentialAction?.Invoke();

            this.AnimationCoroutineIsRunning = false;
            this.AnimationCoroutine = null;
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
        public static void PushSequencesToTop(params GameplaySequenceEvent[] eventsTopush)
        {
            for (int ii = eventsTopush.Length - 1; ii >= 0; ii--)
            {
                PushSequenceToTop(eventsTopush[ii]);
            }
        }

        public static void PushSequenceToTop(GameplaySequenceEvent eventToPush)
        {
            StackedSequenceEvents.Insert(0, eventToPush);
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

        #endregion
    }
}