namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using SFDDCards.Evaluation.Actual;

    public struct ReactionWindowContext
    {
        public string TimingWindowId;

        public CampaignContext CampaignContext;
        public Combatant CombatantEffectOwner;
        public ICombatantTarget CombatantTarget;

        public DeltaEntry ResultingDelta;
        public string PlayedFromZone;

        public ReactionWindowContext(
            CampaignContext campaignContext,
            string timingWindowId,
            DeltaEntry resultingDelta,
            string playedFromZone = null)
        {
            this.CampaignContext = campaignContext;
            this.TimingWindowId = timingWindowId.ToLower();

            this.ResultingDelta = resultingDelta;
            this.CombatantEffectOwner = resultingDelta.User;
            this.CombatantTarget = resultingDelta.Target;

            this.PlayedFromZone = playedFromZone;
        }

        public ReactionWindowContext(
            CampaignContext campaignContext,
            string timingWindowId, 
            Combatant combatantEffectOwner,
            ICombatantTarget combatantTarget = null,
            string playedFromZone = null)
        {
            this.CampaignContext = campaignContext;
            this.TimingWindowId = timingWindowId.ToLower();
            this.CombatantEffectOwner = combatantEffectOwner;
            this.CombatantTarget = combatantTarget;
            this.PlayedFromZone = playedFromZone;

            this.ResultingDelta = null;
        }

        public static readonly ReactionWindowContext LookingNotPlayingContext =
            new ReactionWindowContext()
            {
                CampaignContext = null,
                CombatantEffectOwner = null,
                CombatantTarget = null,
                PlayedFromZone = "hand",
                ResultingDelta = null,
                TimingWindowId = KnownReactionWindows.LookingNotPlaying
            };
    }
}