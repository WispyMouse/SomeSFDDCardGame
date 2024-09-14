using UnityEngine;

namespace SFDDCards
{
    public class FoeTarget : ICombatantTarget
    {
        public string Name => "Foe";
        public Transform UXPositionalTransform { get; set; }

        public void ApplyDelta(CombatContext combatContext, DeltaEntry deltaEntry)
        {
            // No-op
        }

        public bool IsFoeOf(ICombatantTarget otherTarget)
        {
            return true;
        }
    }
}