namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class CombatCardUX : DisplayedCardUX
    {
        [SerializeReference]
        private GameObject SelectedGlow;

        public override void EnableSelectionGlow()
        {
            this.SelectedGlow.SetActive(true);
        }

        public override void DisableSelectionGlow()
        {
            this.SelectedGlow.SetActive(false);
        }
    }
}