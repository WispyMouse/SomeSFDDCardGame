using UnityEngine;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards
{
    public class NoTarget : ICombatantTarget
    {
        public string Name => "No Target";
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