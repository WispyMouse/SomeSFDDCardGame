namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;


    public static class GlobalSequenceEventHolder
    {
        public static readonly List<GameplaySequenceEvent> StackedSequenceEvents = new List<GameplaySequenceEvent>();
        public static GameplaySequenceEvent CurrentSequenceEvent { get; set; } = null;

        public static event Action OnStopAllSequences;

        public static void SynchronouslyResolveAllEvents()
        {
            while (StackedSequenceEvents.Count > 0)
            {
                CurrentSequenceEvent = StackedSequenceEvents[0];
                StackedSequenceEvents.RemoveAt(0);
                CurrentSequenceEvent.ConsequentialAction?.Invoke();
                CurrentSequenceEvent = null;
            }
        }

        public static void ApplyNextSequenceWithAnimationHandler(CombatTurnController controller)
        {
            if (StackedSequenceEvents.Count == 0)
            {
                return;
            }

            CurrentSequenceEvent = StackedSequenceEvents[0];
            StackedSequenceEvents.RemoveAt(0);

            if (CurrentSequenceEvent.AnimationDelegate != null)
            {
                controller.HandleSequenceEventWithAnimation(CurrentSequenceEvent);
            }
            else
            {
                CurrentSequenceEvent.ConsequentialAction?.Invoke();
                CurrentSequenceEvent = null;
            }
        }

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
            OnStopAllSequences.Invoke();
            StackedSequenceEvents.Clear();
            CurrentSequenceEvent = null;
        }

        #endregion
    }
}