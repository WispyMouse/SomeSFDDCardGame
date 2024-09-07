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

        [SerializeField]
        private TMPro.TMP_Text Name;

        [SerializeField]
        private TMPro.TMP_Text Health;

        public void OnMouseDown()
        {
            this.enemySelectedEvent.Invoke(this.RepresentedEnemy);
        }

        public void SetFromEnemy(Enemy toSet, Action<Enemy> inEnemySelectedEvent)
        {
            this.RepresentedEnemy = toSet;
            this.enemySelectedEvent = inEnemySelectedEvent;
            this.Name.text = toSet.Name;
            this.UpdateUX();
        }

        public void UpdateUX()
        {
            this.Health.text = $"{this.RepresentedEnemy.CurrentHealth} / {this.RepresentedEnemy.MaximumHealth}";
        }
    }
}