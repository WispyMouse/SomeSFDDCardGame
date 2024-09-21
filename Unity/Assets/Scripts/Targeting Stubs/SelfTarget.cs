using UnityEngine;

namespace SFDDCards
{
    public class SelfTarget : ICombatantTarget
    {
        public string Name => "Self";
        public Transform UXPositionalTransform { get; set; }

        public void ApplyDelta(CombatContext combatContext, DeltaEntry deltaEntry)
        {
            // No-op
        }

        public int CountStacks(string countFor)
        {
            return 0;
        }

        public int GetTotalHealth()
        {
            return 0;
        }

        public bool IsFoeOf(ICombatantTarget otherTarget)
        {
            return false;
        }

        public bool Valid()
        {
            return true;
        }
    }
}