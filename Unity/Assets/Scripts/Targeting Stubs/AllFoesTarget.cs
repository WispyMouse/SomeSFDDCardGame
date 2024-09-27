using SFDDCards.Evaluation.Actual;
using System.Collections.Generic;
using UnityEngine;

namespace SFDDCards
{
    public class AllFoesTarget : ICombatantTarget
    {
        public string Name => "All Foes";
        public Transform UXPositionalTransform { get; set; }
        public List<ICombatantTarget> AffectedTargets { get; set; } = new List<ICombatantTarget>();

        public AllFoesTarget(List<ICombatantTarget> foes)
        {
            this.AffectedTargets = foes;
        }

        public void ApplyDelta(CampaignContext campaignContext, CombatContext combatContext, DeltaEntry deltaEntry)
        {
            foreach (ICombatantTarget target in new List<ICombatantTarget>(this.AffectedTargets))
            {
                target.ApplyDelta(campaignContext, combatContext, deltaEntry);
            }
        }

        public int CountStacks(string countFor)
        {
            int totalStacks = 0;

            foreach (ICombatantTarget target in new List<ICombatantTarget>(this.AffectedTargets))
            {
                totalStacks += target.CountStacks(countFor);
            }

            return totalStacks;
        }

        public bool IsFoeOf(ICombatantTarget otherTarget)
        {
            if (this.AffectedTargets.Count == 0)
            {
                return false;
            }

            return this.AffectedTargets[0].IsFoeOf(otherTarget);
        }

        public bool Valid()
        {
            return true;
        }

        public int GetTotalHealth()
        {
            int totalHealth = 0;

            foreach (ICombatantTarget target in this.AffectedTargets)
            {
                totalHealth += target.GetTotalHealth();
            }

            return totalHealth;
        }

        public bool Equals(ICombatantTarget other)
        {
            return other != null && other.GetType() == this.GetType();
        }

        public int GetRepresentingNumberOfTargets()
        {
            return this.AffectedTargets.Count;
        }

        public bool IncludesTarget(Combatant target)
        {
            return this.AffectedTargets.Exists(x => x.Equals(target));
        }

        public bool OverlapsTarget(Combatant perspective, ICombatantTarget target)
        {
            foreach (ICombatantTarget affectedTarget in this.AffectedTargets)
            {
                if (!affectedTarget.OverlapsTarget(perspective, target))
                {
                    return false;
                }
            }

            return true;
        }
    }
}