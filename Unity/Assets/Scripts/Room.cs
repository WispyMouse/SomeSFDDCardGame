namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class Room
    {
        public Encounter BasedOnEncounter;
        public List<Enemy> Enemies { get; protected set; } = new List<Enemy>();

        public Room(Encounter basedOnEncounter)
        {
            this.BasedOnEncounter = basedOnEncounter;

            foreach (string id in this.BasedOnEncounter.EnemiesInEncounterById)
            {
                this.Enemies.Add(new Enemy(EnemyDatabase.GetModel(id)));
            }
        }

        public void AddEnemy(Enemy toAdd)
        {
            this.Enemies.Add(toAdd);
        }
    }
}