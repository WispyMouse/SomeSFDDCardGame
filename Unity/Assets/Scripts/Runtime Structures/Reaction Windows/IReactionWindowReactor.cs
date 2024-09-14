namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IReactionWindowReactor
    {
        bool TryGetReactionEvents(CombatContext combatContext, ReactionWindowContext reactionContext, out List<GameplaySequenceEvent> events);
    }
}