namespace SFDDCards.Evaluation.Conceptual
{
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Describes a collection of gamestate change that would result from an effect being used, conceptually.
    /// This is meant to describe an effect in concept, but not with actual targets.
    /// This can be used to describe cards and status effects, while they're not being actively resolved.
    /// </summary>
    public class ConceptualDelta
    {
        public List<ConceptualDeltaEntry> DeltaEntries { get; set; } = new List<ConceptualDeltaEntry>();
        public IEffectOwner Owner;
    }
}