namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AppliedStatusEffect
    {
        public readonly StatusEffect BasedOnStatusEffect;
        public int Stacks { get; set; }

        public AppliedStatusEffect(StatusEffect basedOnEffect, int stacks = 0)
        {
            this.BasedOnStatusEffect = basedOnEffect;
            this.Stacks = stacks;
        }
    }
}