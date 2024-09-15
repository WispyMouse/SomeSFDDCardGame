namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class MouseHoverShowerController : MonoBehaviour
    {
        public IMouseHoverListener CurrentListener;

        [SerializeReference]
        private MouseHoverShowerPanel CanvasSpaceHoverUX;

        private void Awake()
        {
            this.DismissCurrentHover();
        }

        private void OnEnable()
        {
            GlobalUpdateUX.MouseStartHoveredEvent.AddListener(this.HandleShowingHover);
            GlobalUpdateUX.MouseEndHoveredEvent.AddListener(this.HandleEndHover);
        }

        private void OnDisable()
        {
            GlobalUpdateUX.MouseStartHoveredEvent.RemoveListener(this.HandleShowingHover);
            GlobalUpdateUX.MouseEndHoveredEvent.RemoveListener(this.HandleEndHover);
        }

        public void HandleShowingHover(IMouseHoverListener listener)
        {
            if (this.CurrentListener == listener)
            {
                return;
            }

            this.DismissCurrentHover();
            this.CurrentListener = listener;

            this.CanvasSpaceHoverUX.gameObject.SetActive(true);
            this.CanvasSpaceHoverUX.SetFromHoverListener(listener);

            Vector3 screenPosition = Vector3.zero;
            Transform listenerTransform = listener.GetTransform();

            if (listenerTransform is RectTransform rectTransform)
            {
                screenPosition = rectTransform.position;
            }
            else
            {
                screenPosition = Camera.main.WorldToScreenPoint(listenerTransform.position);
            }

            this.CanvasSpaceHoverUX.transform.position = screenPosition;

            this.PlaceHoverAwayFromScreenEdge();
        }

        public void HandleEndHover(IMouseHoverListener listener)
        {
            if (this.CurrentListener != listener)
            {
                return;
            }

            this.DismissCurrentHover();
        }

        private void DismissCurrentHover()
        {
            this.CurrentListener = null;
            this.CanvasSpaceHoverUX.gameObject.SetActive(false);
        }

        private void PlaceHoverAwayFromScreenEdge()
        {
            const float MinimumEdgeSpace = 20f;

            float minX = MinimumEdgeSpace;
            float maxX = Screen.width - MinimumEdgeSpace;
            float minY = MinimumEdgeSpace;
            float maxY = Screen.height - MinimumEdgeSpace;

            float left = this.CanvasSpaceHoverUX.OwnTransform.position.x + this.CanvasSpaceHoverUX.OwnTransform.rect.xMin;
            float right = this.CanvasSpaceHoverUX.OwnTransform.position.x + this.CanvasSpaceHoverUX.OwnTransform.rect.xMax;
            float top = this.CanvasSpaceHoverUX.OwnTransform.position.y + this.CanvasSpaceHoverUX.OwnTransform.rect.yMax;
            float bottom = this.CanvasSpaceHoverUX.OwnTransform.position.y + this.CanvasSpaceHoverUX.OwnTransform.rect.yMin;

            if (left < minX)
            {
                this.CanvasSpaceHoverUX.OwnTransform.position += Vector3.right * (minX - left);
            }

            if (right > maxX)
            {
                this.CanvasSpaceHoverUX.OwnTransform.position += Vector3.left * (right - maxX);
            }

            if (top > maxY)
            {
                this.CanvasSpaceHoverUX.OwnTransform.position += Vector3.down * (top - minY);
            }

            if (bottom < minY)
            {
                this.CanvasSpaceHoverUX.OwnTransform.position += Vector3.up * (minY - bottom);
            }
        }
    }
}