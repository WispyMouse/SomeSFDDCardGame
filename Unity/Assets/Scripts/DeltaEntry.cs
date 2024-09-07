namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    public class DeltaEntry
    {
        public ICombatantTarget User;
        public ICombatantTarget Target;
        public int Intensity;
        public TokenEvaluatorBuilder.IntensityKind IntensityKindType = TokenEvaluatorBuilder.IntensityKind.None;

        public string DescribeDelta()
        {
            if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
            {
                return $"{User.Name} damages {Target.Name} for {Intensity}";
            }

            return "I have no idea what happened.";
        }

        public string DescribeAsEffect()
        {
            if (IntensityKindType == TokenEvaluatorBuilder.IntensityKind.Damage)
            {
                return $"Damages one target for {Intensity}";
            }

            return "I have no idea what this will do.";
        }
    }
}