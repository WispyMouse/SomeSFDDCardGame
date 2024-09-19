namespace SFDDCards.UX
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ChoiceNodeOptionUX : MonoBehaviour
    {
        public ChoiceNodeOption Representing;
        private ChoiceNodeSelectorUX SelectorUx;

        [SerializeReference]
        private TMPro.TMP_Text NameLabel;
        [SerializeReference]
        private TMPro.TMP_Text DescriptionLabel;

        public void RepresentOption(ChoiceNodeSelectorUX selector, ChoiceNodeOption toRepresent)
        {
            this.SelectorUx = selector;
            this.Representing = toRepresent;

            this.NameLabel.text = toRepresent.GetName();
            this.DescriptionLabel.text = toRepresent.GetDescription();
        }

        public void ChooseThis()
        {
            this.SelectorUx.NodeIsChosen(this);
        }
    }
}