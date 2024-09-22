namespace SFDDCards
{
    using SFDDCards.ImportModels;
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

        public EnemyModel(EnemyImport basedOn)
        {
            this.Id = basedOn.Id.ToLower();
            this.Name = basedOn.Name;
            this.MaximumHealth = basedOn.MaximumHealth;

            HashSet<string> lowerCaseTags = new HashSet<string>();
            foreach (string tag in basedOn.Tags)
            {
                lowerCaseTags.Add(tag.ToLower());
            }
            this.Tags = lowerCaseTags;

            foreach (EnemyAttackImport attack in basedOn.Attacks)
            {
                this.Attacks.Add(new EnemyAttack(attack));
            }
        }

        public bool MeetsAllTags(HashSet<string> tags)
        {
            return this.Tags.Overlaps(tags);
        }
    }
}