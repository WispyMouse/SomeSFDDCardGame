namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public abstract class Combatant : ICombatantTarget
    {
        public abstract string Name { get; }
        public abstract int MaxHealth { get; }
        public int CurrentHealth { get; set; }
        public Transform UXPositionalTransform { get; set; }

        public void ApplyDelta(DeltaEntry deltaEntry)
        {
            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
            {
                this.CurrentHealth = Mathf.Max(0, this.CurrentHealth - deltaEntry.Intensity);
                return;
            }

            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Heal)
            {
                this.CurrentHealth = Mathf.Min(this.MaxHealth, this.CurrentHealth + deltaEntry.Intensity);
                return;
            }
        }

        public bool IsFoeOf(ICombatantTarget otherTarget)
        {
            if (this is Enemy)
            {
                return otherTarget is Player;
            }
            else if (this is Player)
            {
                return otherTarget is Enemy;
            }
            return false;
        }
    }
}