namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class Enemy
    {
        public string Name;
        public int MaximumHealth;
        public int CurrentHealth;

        public bool ShouldBecomeDefeated
        {
            get
            {
                return this.CurrentHealth <= 0;
            }
        }
    }
}