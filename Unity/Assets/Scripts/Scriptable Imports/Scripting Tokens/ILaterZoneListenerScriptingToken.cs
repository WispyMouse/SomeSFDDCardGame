namespace SFDDCards.Evaluation.Conceptual
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ScriptingTokens;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using static SFDDCards.Evaluation.Actual.TokenEvaluatorBuilder;
    public interface ILaterZoneListenerScriptingToken : IScriptingToken
    {
        /// <summary>
        /// Other scripting tokens can look backwards to see this token; this can then be used to describe its destination
        /// </summary>
        public string LaterRealizedDestinationZone { get; set; }

        public bool ShouldSilenceSpeaker { get; }
    }
}
