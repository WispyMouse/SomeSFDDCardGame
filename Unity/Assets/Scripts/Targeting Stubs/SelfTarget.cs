using UnityEngine;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards
{
    public class SelfTarget : ICombatantTarget
    {
        public string Name => "Self";
        public Transform UXPositionalTransform { get; set; }

        public void ApplyDelta(CampaignContext campaignContext, CombatContext combatContext, DeltaEntry deltaEntry)
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

        public bool Equals(ICombatantTarget other)
        {
            return other != null && other.GetType() == this.GetType();
        }

        public int GetRepresentingNumberOfTargets()
        {
            return 1;
        }
    }
}