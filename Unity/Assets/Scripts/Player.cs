namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class Player : ICombatantTarget
    {
        public int MaxHealth;
        public int CurrentHealth { get; set; }

        public string Name => "Player";

        public void ApplyDelta(DeltaEntry deltaEntry)
        {
            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
            {
                this.CurrentHealth = Mathf.Max(this.CurrentHealth - deltaEntry.Intensity, 0);
            }
        }
    }
}