namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public class TokenEvaluatorBuilder
    {
        public enum IntensityKind
        {
            None = 0,
            Damage = 1
        }

        public List<IScriptingToken> AppliedTokens = new List<IScriptingToken>();

        public bool ShouldLaunch = false;

        public ICombatantTarget Target;
        public ICombatantTarget User;

        public int Intensity;
        public IntensityKind IntensityKindType;

        public GamestateDelta GetEffectiveDelta()
        {
            GamestateDelta delta = new GamestateDelta();

            delta.DeltaEntries.Add(new DeltaEntry()
            {
                User = this.User,
                Target = this.Target,
                Intensity = this.Intensity,
                IntensityKindType = this.IntensityKindType
            });

            return delta;
        }
    }
}