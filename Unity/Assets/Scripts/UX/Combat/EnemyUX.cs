namespace SFDDCards.UX
{
    using SFDDCards.Evaluation.Actual;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class EnemyUX : MonoBehaviour, IAnimationPuppet
    {
        public Enemy RepresentedEnemy { get; private set; } = null;

        public Transform OwnTransform => this.transform;

        public bool IsNotDestroyed => this != null && this?.gameObject != null;

        [SerializeReference]
        private TMPro.TMP_Text Name;

        [SerializeReference]
        private TMPro.TMP_Text Health;

        [SerializeReference]
        private TMPro.TMP_Text EffectText;

        [SerializeReference]
        private EnemyStatusEffectUXHolder OwnStatusEffectHolder;
        public void SetFromEnemy(Enemy toSet, CentralGameStateController centralGameStateController)
        {
            this.RepresentedEnemy = toSet;
            this.Name.text = toSet.BaseModel.Name;
            this.RepresentedEnemy.UXPositionalTransform = this.transform;

            this.ClearEffectText();
            this.OwnStatusEffectHolder.Annihilate();

            this.UpdateUX(centralGameStateController);
        }

        public void UpdateUX(CentralGameStateController centralGameStateController)
        {
            this.Health.text = $"{this.RepresentedEnemy.CurrentHealth} / {this.RepresentedEnemy.BaseModel.MaximumHealth}";
            this.UpdateIntent(centralGameStateController);

            this.OwnStatusEffectHolder.SetStatusEffects(this.RepresentedEnemy.AppliedStatusEffects);
        }

        public void SetEffectText(string toText)
        {
            this.EffectText.text = toText;
            this.EffectText.gameObject.SetActive(true);
        }

        public void ClearEffectText()
        {
            this.EffectText.gameObject.SetActive(false);
        }

        void UpdateIntent(CentralGameStateController centralGameStateController)
        {
            string description = "";

            if (this.RepresentedEnemy.Intent != null)
            {
                description = EffectDescriberDatabase.DescribeRealizedEffect(
                    this.RepresentedEnemy.Intent,
                    centralGameStateController.CurrentCampaignContext,
                    this.RepresentedEnemy,
                    centralGameStateController.CurrentCampaignContext.CurrentCombatContext.CombatPlayer
                    );
            }

            if (!string.IsNullOrEmpty(description))
            {
                this.SetEffectText(description);
            }
            else
            {
                this.ClearEffectText();
            }
        }
    }
}