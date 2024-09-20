namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class MouseHoverShowerPanel : MonoBehaviour
    {
        [SerializeReference]
        private RenderedCard OverlayCard;

        [SerializeReference]
        public RectTransform OwnTransform;

        [SerializeReference]
        private PopupPanel PopupPanelPF;

        [SerializeReference]
        private Transform RightPopupPanelHolderTransform;

        [SerializeReference]
        private Transform LeftPopupPanelHolderTransform;

        private List<PopupPanel> ActivePanels { get; set; } = new List<PopupPanel>();

        public void SetFromHoverListener(IMouseHoverListener listener)
        {
            if (listener.TryGetCard(out Card toShow))
            {
                this.ShowCard(toShow);
            }
            else
            {
                this.HideCard();
            }

            foreach (Graphic curGraphic in this.GetComponentsInChildren<Graphic>(true))
            {
                curGraphic.raycastTarget = false;
            }
        }

        private void ShowCard(Card toShow)
        {
            this.OverlayCard.gameObject.SetActive(true);
            this.OverlayCard.SetFromCard(toShow);
            this.SetPopupPanelsFromCard(toShow);
        }

        private void HideCard()
        {
            this.OverlayCard.Annihilate();
            this.OverlayCard.gameObject.SetActive(false);
        }
        private void SetPopupPanelsFromCard(Card toShow)
        {
            List<string> texts = new List<string>();

            texts.Add($"<b>Tags:</b> {string.Join(", ", toShow.Tags)}");

            this.SetPopupPanels(texts);
        }

        private void SetPopupPanels(List<string> text)
        {
            for (int ii = this.RightPopupPanelHolderTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.RightPopupPanelHolderTransform.GetChild(ii).gameObject);
            }

            for (int ii = this.LeftPopupPanelHolderTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.LeftPopupPanelHolderTransform.GetChild(ii).gameObject);
            }

            for (int ii = this.ActivePanels.Count - 1; ii >= 0; ii--)
            {
                if (this.ActivePanels[ii] != null)
                { 
                    Destroy(this.ActivePanels[ii].gameObject);
                }
            }

            this.ActivePanels.Clear();
            Transform holder = this.RightPopupPanelHolderTransform;
            bool usingLeftHolder = false;

            if (this.transform.position.x >= Screen.width / 2f)
            {
                holder = this.LeftPopupPanelHolderTransform;
                usingLeftHolder = true;
            }

            foreach (string currentText in text)
            {
                PopupPanel panel = Instantiate(this.PopupPanelPF, holder);
                panel.SetText(currentText);
                this.ActivePanels.Add(panel);

                if (usingLeftHolder)
                {
                    panel.LayoutGroup.childAlignment = TextAnchor.UpperRight;
                }
            }
        }

        public List<RectTransform> GetAllRectTransforms()
        {
            List<RectTransform> transforms = new List<RectTransform>();

            transforms.Add(this.OverlayCard.GetComponent<RectTransform>());

            foreach (PopupPanel panel in this.ActivePanels)
            {
                transforms.Add(panel.GetComponent<RectTransform>());
            }

            return transforms;
        }
    }
}