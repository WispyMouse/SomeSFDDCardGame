namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class EnemyUX : MonoBehaviour
    {
        public Enemy RepresentedEnemy { get; private set; } = null;
        Action<Enemy> enemySelectedEvent { get; set; }

        public void OnMouseDown()
        {
            this.enemySelectedEvent.Invoke(this.RepresentedEnemy);
        }

        public void SetFromEnemy(Enemy toSet, Action<Enemy> inEnemySelectedEvent)
        {
            this.RepresentedEnemy = toSet;
            this.enemySelectedEvent = inEnemySelectedEvent;
        }
    }
}