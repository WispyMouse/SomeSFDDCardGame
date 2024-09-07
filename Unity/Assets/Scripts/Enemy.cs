namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class Enemy : ICombatantTarget
    {
        public EnemyModel BaseModel { get; private set; }
        public int CurrentHealth { get; set; }

        public bool ShouldBecomeDefeated
        {
            get
            {
                return this.CurrentHealth <= 0;
            }
        }

        public string Name => this.BaseModel.Name;

        public Enemy(EnemyModel baseModel)
        {
            this.BaseModel = baseModel;
            this.CurrentHealth = baseModel.MaximumHealth;
        }

        public void ApplyDelta(DeltaEntry deltaEntry)
        {
            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
            {
                this.CurrentHealth = Mathf.Max(this.CurrentHealth - deltaEntry.Intensity, 0);
            }
        }
    }
}