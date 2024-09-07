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
    }
}