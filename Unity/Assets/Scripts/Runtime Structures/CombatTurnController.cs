namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;


    public class CombatTurnController : MonoBehaviour
    {
        [SerializeReference]
        private CentralGameStateController CentralGameStateControllerInstance;

        [SerializeReference]
        private GameplayUXController UXController;

        public bool CurrentlyActive { get; private set; } = false;

        public void BeginHandlingCombat()
        {
            this.CurrentlyActive = true;

            this.SpawnInitialEnemies();
        }

        public void EndHandlingCombat()
        {
            this.CurrentlyActive = false;
        }

        private void SpawnInitialEnemies()
        {
            foreach (Enemy curEnemy in this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.Enemies)
            {
                this.UXController.AddEnemy(curEnemy);
            }
        }
    }
}