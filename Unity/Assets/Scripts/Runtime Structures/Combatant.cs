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
        public List<AppliedStatusEffect> AppliedStatusEffects { get; set; } = new List<AppliedStatusEffect>();

        public void ApplyDelta(DeltaEntry deltaEntry)
        {
            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
            {
                if (deltaEntry.Intensity > 0)
                {
                    this.CurrentHealth = Mathf.Max(0, this.CurrentHealth - deltaEntry.Intensity);
                }
                return;
            }

            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Heal)
            {
                if (deltaEntry.Intensity > 0)
                {
                    this.CurrentHealth = Mathf.Min(this.MaxHealth, this.CurrentHealth + deltaEntry.Intensity);
                }

                return;
            }

            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.StatusEffect)
            {
                AppliedStatusEffect existingEffect = this.AppliedStatusEffects.Find(x => x.BasedOnStatusEffect == deltaEntry.StatusEffect);
                if (existingEffect != null)
                {
                    existingEffect.Stacks += deltaEntry.Intensity;
                    if (existingEffect.Stacks <= 0)
                    {
                        AppliedStatusEffects.Remove(existingEffect);
                    }
                }
                else if (deltaEntry.Intensity > 0)
                {
                    AppliedStatusEffect newEffect = new AppliedStatusEffect(deltaEntry.StatusEffect, deltaEntry.Intensity);
                    this.AppliedStatusEffects.Add(newEffect);
                }
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