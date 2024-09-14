namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public interface ICombatantTarget
    {
        string Name { get; }
        void ApplyDelta(CombatContext combatContext, DeltaEntry deltaEntry);
        Transform UXPositionalTransform { get; }

        bool IsFoeOf(ICombatantTarget otherTarget);
        bool Valid();
        int CountStacks(string countFor);
    }
}