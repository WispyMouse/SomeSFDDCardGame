using UnityEngine;

namespace SFDDCards
{
    public class AbstractPlayerUser : ICombatantTarget
    {
        public string Name => "Player";
        public Transform UXPositionalTransform { get; set; }

        public void ApplyDelta(CombatContext combatContext, DeltaEntry deltaEntry)
        {
            // No-op
        }

        public bool IsFoeOf(ICombatantTarget otherTarget)
        {
            if (otherTarget is Player)
            {
                return false;
            }
            else if (otherTarget is Enemy)
            {
                return true;
            }

            return false;
        }
    }
}