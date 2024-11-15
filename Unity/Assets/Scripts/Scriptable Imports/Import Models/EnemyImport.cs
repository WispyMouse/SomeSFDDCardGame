namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class EnemyImport : Importable
    {
        public string Name;
        public int MaximumHealth;
        public HashSet<string> Tags = new HashSet<string>();

        public List<EnemyAttackImport> Attacks = new List<EnemyAttackImport>();
    }
}