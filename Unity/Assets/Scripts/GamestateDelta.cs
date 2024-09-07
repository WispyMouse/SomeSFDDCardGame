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
                entry.Target.ApplyDelta(entry);
            }
        }

        public void ApplyDelta(GamestateDelta delta)
        {
            this.DeltaEntries.AddRange(delta.DeltaEntries);
        }

        public string DescribeDelta()
        {
            StringBuilder stringLog = new StringBuilder();

            foreach (DeltaEntry entry in DeltaEntries)
            {
                stringLog.AppendLine(entry.DescribeDelta());
            }

            return stringLog.ToString();
        }

        public string DescribeAsEffect()
        {
            StringBuilder stringLog = new StringBuilder();

            foreach (DeltaEntry entry in DeltaEntries)
            {
                stringLog.AppendLine(entry.DescribeAsEffect());
            }

            return stringLog.ToString();
        }
    }
}