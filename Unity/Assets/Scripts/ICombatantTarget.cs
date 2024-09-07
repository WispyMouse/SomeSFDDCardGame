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
        void ApplyDelta(DeltaEntry deltaEntry);
    }
}