namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class Room
    {
        public List<Enemy> Enemies { get; protected set; } = new List<Enemy>();

        public void AddEnemy(Enemy toAdd)
        {
            this.Enemies.Add(toAdd);
        }
    }
}