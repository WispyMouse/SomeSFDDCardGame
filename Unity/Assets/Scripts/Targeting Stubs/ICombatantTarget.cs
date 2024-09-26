namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public interface ICombatantTarget : IEquatable<ICombatantTarget>
    {
        string Name { get; }
        void ApplyDelta(CampaignContext campaignContext, CombatContext combatContext, DeltaEntry deltaEntry);
        Transform UXPositionalTransform { get; }

        bool IsFoeOf(ICombatantTarget otherTarget);
        bool Valid();
        int CountStacks(string countFor);
        int GetTotalHealth();

        int GetRepresentingNumberOfTargets();
        bool IncludesTarget(Combatant target);
        bool OverlapsTarget(Combatant perspective, ICombatantTarget target);
    }
}