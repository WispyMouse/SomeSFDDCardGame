namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IReactionWindowReactor
    {
        bool TryGetReactionEvents(CampaignContext campaignContext, ReactionWindowContext reactionContext, out List<GameplaySequenceEvent> events);
    }
}