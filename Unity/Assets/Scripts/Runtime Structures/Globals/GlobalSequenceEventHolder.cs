namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Events;

    public static class GlobalSequenceEventHolder
    {
        public static readonly List<GameplaySequenceEvent> StackedSequenceEvents = new List<GameplaySequenceEvent>();
        public static GameplaySequenceEvent CurrentSequenceEvent { get; set; } = null;

        public static UnityEvent OnStopAllSequences = new UnityEvent();

        public static void SynchronouslyResolveAllEvents(CampaignContext forCampaign)
        {
            while (StackedSequenceEvents.Count > 0)
            {
                CurrentSequenceEvent = StackedSequenceEvents[0];
                StackedSequenceEvents.RemoveAt(0);
                CurrentSequenceEvent.ConsequentialAction?.Invoke();
                GlobalUpdateUX.UpdateUXEvent.Invoke(forCampaign);
                CurrentSequenceEvent = null;
            }
        }

        public static void ApplyNextSequenceWithAnimationHandler(ICombatTurnController controller)
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
                GlobalUpdateUX.UpdateUXEvent.Invoke(controller.ForCampaign);
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
        public static void PushSequencesToTop(CampaignContext context, params GameplaySequenceEvent[] eventsToPush)
        {
            if (eventsToPush.Length == 0)
            {
                return;
            }

            for (int ii = eventsToPush.Length - 1; ii >= 0; ii--)
            {
                PushSequenceToTop(eventsToPush[ii]);
            }
        }

        public static void PushSequencesToTop(CampaignContext context, params WindowResponse[] responses)
        {
            if (responses.Length == 0)
            {
                return;
            }

            List<WindowResponse> orderedList = new List<WindowResponse>(responses);

            // The compare uses a negative so that higher ApplicationPriority values happen before lower values
            orderedList.Sort((WindowResponse x, WindowResponse y) => -x.FromResponder.ApplicationPriority.CompareTo(y.FromResponder.ApplicationPriority));

            List<GameplaySequenceEvent> sequences = new List<GameplaySequenceEvent>();

            foreach (WindowResponse response in orderedList)
            {
                GamestateDelta delta = ScriptTokenEvaluator.CalculateRealizedDeltaEvaluation(
                    response.FromResponder.Effect, 
                    context,
                    response.FromContext.CombatantEffectOwner,
                    response.FromContext.CombatantEffectOwner, 
                    response.FromContext);
                GlobalUpdateUX.LogTextEvent.Invoke(EffectDescriberDatabase.DescribeResolvedEffect(delta), GlobalUpdateUX.LogType.GameEvent);
                delta.ApplyDelta(context);
            };

            PushSequencesToTop(context, sequences.ToArray());
        }

        public static void PushSequenceToTop(GameplaySequenceEvent eventToPush)
        {
            if (eventToPush == null)
            {
                Debug.LogError($"{nameof(GlobalSequenceEventHolder)} ({nameof(PushSequenceToTop)}): Null event tried to push to top.");
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