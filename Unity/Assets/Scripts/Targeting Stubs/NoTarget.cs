using UnityEngine;

namespace SFDDCards
{
    public class NoTarget : ICombatantTarget
    {
        public string Name => "No Target";
        public Transform UXPositionalTransform { get; set; }

        public void ApplyDelta(DeltaEntry deltaEntry)
        {
            // No-op
        }

        public bool IsFoeOf(ICombatantTarget otherTarget)
        {
            return false;
        }
    }
}