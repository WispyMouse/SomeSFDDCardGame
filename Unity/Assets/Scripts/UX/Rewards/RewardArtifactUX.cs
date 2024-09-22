namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class RewardArtifactUX : MonoBehaviour
    {
        public StatusEffect RepresentedArtifact { get; set; }

        [SerializeReference]
        private Image ImageRepresentation;

        [SerializeReference]
        private TMPro.TMP_Text Name;

        [SerializeReference]
        private TMPro.TMP_Text Label;

        [SerializeReference]
        public LayoutElement OwnLayoutElement;


        private Action<RewardArtifactUX> OnClicked {get; set;} 

        public void SetFromArtifact(StatusEffect artifact, Action<RewardArtifactUX> onClick)
        {
            this.RepresentedArtifact = artifact;

            this.ImageRepresentation.sprite = artifact.Sprite;
            this.Name.text = artifact.Name;
            this.Label.text = artifact.DescribeStatusEffect().BreakDescriptionsIntoString();
            this.OnClicked = onClick;
        }

        public void Clicked()
        {
            this.gameObject.SetActive(false);
            this.OnClicked.Invoke(this);
        }
    }
}