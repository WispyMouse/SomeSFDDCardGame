namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class EnemyModel
    {
        public string Id;
        public string Name;
        public int MaximumHealth;
        public HashSet<string> Tags = new HashSet<string>();

        public List<EnemyAttack> Attacks = new List<EnemyAttack>();

        public bool MeetsAllTags(HashSet<string> tags)
        {
            return this.Tags.Overlaps(tags);
        }
    }
}