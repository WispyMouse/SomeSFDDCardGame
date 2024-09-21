namespace SFDDCards.UX
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public abstract class StatusEffectUXHolder : MonoBehaviour
    {
        [SerializeReference]
        private StatusEffectUX StatusEffectUXPrefab;

        private Dictionary<AppliedStatusEffect, StatusEffectUX> StatusEffectLookup { get; set; } = new Dictionary<AppliedStatusEffect, StatusEffectUX>();

        public void SetStatusEffects(List<AppliedStatusEffect> appliedEffects)
        {
            List<AppliedStatusEffect> noLongerApplicableEffects = new List<AppliedStatusEffect>(this.StatusEffectLookup.Keys);

            foreach (AppliedStatusEffect effect in appliedEffects)
            {
                if (this.StatusEffectLookup.TryGetValue(effect, out StatusEffectUX existingUX))
                {
                    existingUX.SetStacks(effect.Stacks);
                }
                else
                {
                    StatusEffectUX newUX = Instantiate(this.StatusEffectUXPrefab, this.transform);
                    this.StatusEffectLookup.Add(effect, newUX);
                    newUX.SetFromEffect(effect);
                    newUX.SetStacks(effect.Stacks);
                }

                noLongerApplicableEffects.Remove(effect);
            }

            if (noLongerApplicableEffects.Count > 0)
            {
                for (int ii = noLongerApplicableEffects.Count - 1; ii >= 0; ii--)
                {
                    Destroy(this.StatusEffectLookup[noLongerApplicableEffects[ii]].gameObject);
                    this.StatusEffectLookup.Remove(noLongerApplicableEffects[ii]);
                }
            }
        }

        public void Annihilate()
        {
            this.StatusEffectLookup.Clear();

            for (int ii = this.transform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.transform.GetChild(ii).gameObject);
            }
        }
    }
}