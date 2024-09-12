namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public class GamestateDelta
    {
        public List<DeltaEntry> DeltaEntries { get; set; } = new List<DeltaEntry>();

        public void ApplyDelta(CentralGameStateController gameStateController, Action<string> logFunction)
        {
            foreach (DeltaEntry entry in DeltaEntries)
            {
                if (entry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.NumberOfCards)
                {
                    if (entry.NumberOfCardsRelationType == TokenEvaluatorBuilder.NumberOfCardsRelation.Draw)
                    {
                        gameStateController.CurrentDeck.DealCards(entry.Intensity);
                    }
                }

                foreach (ElementResourceChange change in entry.ElementResourceChanges)
                {
                    gameStateController.CurrentCombatContext.ApplyElementResourceChange(change);
                }
                
                entry.Target.ApplyDelta(entry);
            }
        }

        public void AppendDelta(GamestateDelta delta)
        {
            this.DeltaEntries.AddRange(delta.DeltaEntries);
        }

        public string DescribeDelta()
        {
            StringBuilder stringLog = new StringBuilder();

            foreach (DeltaEntry entry in DeltaEntries)
            {
                string description = entry.DescribeDelta();

                if (!string.IsNullOrEmpty(description))
                {
                    stringLog.AppendLine(description);
                }
            }

            return stringLog.ToString();
        }

        public string DescribeAsEffect()
        {
            StringBuilder stringLog = new StringBuilder();

            foreach (DeltaEntry entry in DeltaEntries)
            {
                string description = entry.DescribeAsEffect();

                if (!string.IsNullOrEmpty(description))
                {
                    stringLog.AppendLine(description);
                }
            }

            return stringLog.ToString();
        }

        public void EvaluateVariables(CentralGameStateController controller, ICombatantTarget user, ICombatantTarget target)
        {
            for (int ii = 0; ii < this.DeltaEntries.Count; ii++)
            {
                DeltaEntry curEntry = this.DeltaEntries[ii];

                curEntry.OriginalTarget = target;
                curEntry.User = user;
                
                if (curEntry.AbstractTarget != null)
                {
                    curEntry.AbstractTarget.TryEvaluateValue(controller, curEntry.MadeFromBuilder, out curEntry.Target);
                    curEntry.AbstractTarget = null;
                }

                if (curEntry.AbstractIntensity != null)
                {
                    curEntry.AbstractIntensity.TryEvaluateValue(controller, curEntry.MadeFromBuilder, out curEntry.Intensity);
                    curEntry.AbstractIntensity = null;
                }
            }
        }
    }
}