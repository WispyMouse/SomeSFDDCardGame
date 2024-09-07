namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    [Serializable]
    public class EnemyAttackImport
    {
        public string AttackScript;

        public EnemyAttack DeriveAttack()
        {
            EnemyAttack attack = new EnemyAttack();

            attack.AttackTokens = ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(AttackScript);

            return attack;
        }
    }
}