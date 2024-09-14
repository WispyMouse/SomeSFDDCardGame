namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class EnemyImport
    {
        public string Id;
        public string Name;
        public int MaximumHealth;

        public List<EnemyAttackImport> Attacks;

        public EnemyModel DeriveEnemyModel()
        {
            EnemyModel newEnemy = new EnemyModel();
            newEnemy.Id = this.Id.ToLower();
            newEnemy.Name = this.Name;
            newEnemy.MaximumHealth = this.MaximumHealth;

            foreach (EnemyAttackImport attack in this.Attacks)
            {
                newEnemy.Attacks.Add(attack.DeriveAttack());
            }

            return newEnemy;
        }
    }
}