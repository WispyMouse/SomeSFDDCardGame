namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class Player : Combatant
    {
        public override string Name => "Player";
        public override int MaxHealth { get; }

        public Player(int maxHealth)
        {
            this.MaxHealth = maxHealth;
            this.CurrentHealth = maxHealth;
        }
    }
}