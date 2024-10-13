namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public abstract class Combatant : ICombatantTarget, IReactionWindowReactor
    {
        public abstract string Id { get; }

        public abstract string Name { get; }
        public abstract int MaxHealth { get; }
        public int CurrentHealth { get; set; }
        public Transform UXPositionalTransform { get; set; }
        public virtual List<AppliedStatusEffect> AppliedStatusEffects { get; set; } = new List<AppliedStatusEffect>();

        public void ApplyDelta(CampaignContext campaignContext, CombatContext combatContext, DeltaEntry deltaEntry)
        {
            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.None)
            {
                return;
            }

            if (!deltaEntry.ConceptualIntensity.TryEvaluateValue(campaignContext, deltaEntry.MadeFromBuilder, out int evaluatedIntensity))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse intensity.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
            {
                if (evaluatedIntensity > 0)
                {
                    this.CurrentHealth = Mathf.Max(0, this.CurrentHealth - evaluatedIntensity);
                }

                GlobalSequenceEventHolder.PushSequenceToTop(new GameplaySequenceEvent(() =>
                {
                    campaignContext.CheckAndApplyReactionWindow(new ReactionWindowContext(deltaEntry.FromCampaign, KnownReactionWindows.DamageDealt, deltaEntry));
                }));

                return;
            }

            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Heal)
            {
                if (evaluatedIntensity > 0)
                {
                    this.CurrentHealth = Mathf.Min(this.MaxHealth, this.CurrentHealth + evaluatedIntensity);
                }

                return;
            }

            if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.ApplyStatusEffect)
            {
                AppliedStatusEffect existingEffect = this.AppliedStatusEffects.Find(x => x.BasedOnStatusEffect == deltaEntry.StatusEffect);
                if (existingEffect != null)
                {
                    existingEffect.Stacks += Mathf.Max(0, evaluatedIntensity);
                    if (existingEffect.Stacks <= 0)
                    {
                        campaignContext.UnsubscribeReactor(existingEffect);
                        AppliedStatusEffects.Remove(existingEffect);
                    }
                }
                else if (evaluatedIntensity > 0)
                {
                    AppliedStatusEffect newEffect = new AppliedStatusEffect(this, deltaEntry.StatusEffect, evaluatedIntensity);
                    this.AppliedStatusEffects.Add(newEffect);
                    newEffect.SetSubscriptions(campaignContext);
                }
            }
            else if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.RemoveStatusEffect)
            {
                AppliedStatusEffect existingEffect = this.AppliedStatusEffects.Find(x => x.BasedOnStatusEffect == deltaEntry.StatusEffect);
                if (existingEffect != null)
                {
                    existingEffect.Stacks = Mathf.Max(0, existingEffect.Stacks - evaluatedIntensity);
                    if (existingEffect.Stacks <= 0)
                    {
                        campaignContext.UnsubscribeReactor(existingEffect);
                        AppliedStatusEffects.Remove(existingEffect);
                    }
                }
            }
            else if (deltaEntry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.SetStatusEffect)
            {
                AppliedStatusEffect existingEffect = this.AppliedStatusEffects.Find(x => x.BasedOnStatusEffect == deltaEntry.StatusEffect);
                if (existingEffect != null)
                {
                    existingEffect.Stacks = evaluatedIntensity;
                    if (existingEffect.Stacks <= 0)
                    {
                        campaignContext.UnsubscribeReactor(existingEffect);
                        AppliedStatusEffects.Remove(existingEffect);
                    }
                }
                else if (evaluatedIntensity > 0)
                {
                    AppliedStatusEffect newEffect = new AppliedStatusEffect(this, deltaEntry.StatusEffect, evaluatedIntensity);
                    this.AppliedStatusEffects.Add(newEffect);
                    newEffect.SetSubscriptions(campaignContext);
                }
            }

            GlobalUpdateUX.UpdateUXEvent.Invoke();
        }

        public bool TryGetReactionEvents(CampaignContext campaignContext, ReactionWindowContext reactionContext, out List<WindowResponse> responses)
        {
            responses = null;
            return false;
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

        public bool Valid()
        {
            return this.CurrentHealth > 0;
        }

        public int CountStacks(string countFor)
        {
            AppliedStatusEffect effect = this.AppliedStatusEffects.Find(x => x.BasedOnStatusEffect.Id.ToLower() == countFor.ToLower());

            if (effect == null)
            {
                return 0;
            }

            return effect.Stacks;
        }

        public int GetTotalHealth()
        {
            return this.CurrentHealth;
        }

        public bool Equals(ICombatantTarget other)
        {
            return this == other;
        }

        public int GetRepresentingNumberOfTargets()
        {
            return 1;
        }

        public bool IncludesTarget(Combatant target)
        {
            return this.Equals(target);
        }

        public bool OverlapsTarget(Combatant perspective, ICombatantTarget target)
        {
            return this.Equals(target);
        }
    }
}