namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class ReactionWindowSubscription
    {
        public IReactionWindowReactor Reactor { get; protected set; }
        public abstract string ReactionWindowId { get; }
        public virtual bool ShouldApply(ReactionWindowContext context)
        {
            return true;
        }
    }
}