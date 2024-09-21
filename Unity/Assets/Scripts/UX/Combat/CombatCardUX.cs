namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class CombatCardUX : DisplayedCardUX
    {
        [SerializeReference]
        private GameObject RequirementsMetSelectedGlow;

        [SerializeReference]
        private GameObject NoRequirementsMetSelectedGlow;

        [SerializeReference]
        private GameObject NoRequirementsMetPassiveOverlay;

        public bool RequirementsAreMet
        {
            get
            {
                return this.requirementsAreMet;
            }
            set
            {
                this.requirementsAreMet = value;
                this.UpdateRequirementsGlow();
            }
        }
        private bool requirementsAreMet { get; set; } = true;
        private bool IsGlowing { get; set; } = false;

        private void UpdateRequirementsGlow()
        {
            if (this.RequirementsAreMet)
            {
                this.NoRequirementsMetPassiveOverlay.SetActive(false);
            }
            else
            {
                this.NoRequirementsMetPassiveOverlay.SetActive(true);
            }

            if (this.IsGlowing)
            {
                this.EnableSelectionGlow();
            }
            else
            {
                this.DisableSelectionGlow();
            }
        }

        public override void EnableSelectionGlow()
        {
            if (RequirementsAreMet)
            {
                this.RequirementsMetSelectedGlow.SetActive(true);
                this.NoRequirementsMetSelectedGlow.SetActive(false);
            }
            else
            {
                this.RequirementsMetSelectedGlow.SetActive(false);
                this.NoRequirementsMetSelectedGlow.SetActive(true);
            }

            this.IsGlowing = true;
        }

        public override void DisableSelectionGlow()
        {
            this.IsGlowing = false;
            this.NoRequirementsMetSelectedGlow.SetActive(false);
            this.RequirementsMetSelectedGlow.SetActive(false);
        }
    }
}