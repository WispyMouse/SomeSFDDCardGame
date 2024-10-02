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

        [SerializeField]
        private float LerpTimeMinimumSeconds = .15f;

        [SerializeField]
        private float LerpTimeMaximumSeconds = .6f;

        [SerializeField]
        private float DistanceForMaximumLerp = 4f;

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

        private Vector3 _targetScreenspace { get; set; } = Vector3.zero;
        private float _targetRotation { get; set; } = 0;
        private Vector3? _screenPositionAtStartOfLerp { get; set; } = null;
        private float _rotationAtStartOfLerp { get; set; } = 0;
        private float currentPositionLerpTime { get; set; } = 0;
        private float totalPositionLerpTime { get; set; } = 0;

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

        public void SetTargetPosition(Vector3 position, float rotation)
        {
            this._targetScreenspace = position;
            this._screenPositionAtStartOfLerp = this.transform.position;
            this._rotationAtStartOfLerp = this.transform.rotation.eulerAngles.z;
            this._targetRotation = rotation;

            float distance = (transform.position - position).magnitude;
            if (distance > 0)
            {
                this.totalPositionLerpTime = Mathf.Lerp(this.LerpTimeMinimumSeconds, this.LerpTimeMaximumSeconds, distance / this.DistanceForMaximumLerp);
                this.currentPositionLerpTime = 0;
            }
        }

        public void SnapToPosition(Vector3 screenPosition)
        {
            this.transform.position = screenPosition;
            this._screenPositionAtStartOfLerp = screenPosition;
            this._targetScreenspace = screenPosition;
        }

        private void Update()
        {
            if (!_screenPositionAtStartOfLerp.HasValue)
            {
                return;
            }

            this.currentPositionLerpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(this._screenPositionAtStartOfLerp.Value, this._targetScreenspace, currentPositionLerpTime / totalPositionLerpTime);
            transform.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(this._rotationAtStartOfLerp, this._targetRotation, currentPositionLerpTime / totalPositionLerpTime));

            if (this.currentPositionLerpTime >= this.totalPositionLerpTime)
            {
                this._screenPositionAtStartOfLerp = null;
            }
        }
    }
}