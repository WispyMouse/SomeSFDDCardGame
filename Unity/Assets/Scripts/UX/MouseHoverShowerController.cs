namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class MouseHoverShowerController : MonoBehaviour
    {
        public static UnityEvent<IMouseHoverListener> MouseStartHoveredEvent = new UnityEvent<IMouseHoverListener>();
        public static UnityEvent<IMouseHoverListener> MouseEndHoveredEvent = new UnityEvent<IMouseHoverListener>();

        public IMouseHoverListener CurrentListener;

        [SerializeReference]
        private MouseHoverShowerPanel CanvasSpaceHoverUX;

        private void Awake()
        {
            this.DismissCurrentHover();
        }

        private void Update()
        {
            if (this.CurrentListener == null)
            {
                this.DismissCurrentHover();
            }
        }

        private void OnEnable()
        {
            MouseStartHoveredEvent.AddListener(this.HandleShowingHover);
            MouseEndHoveredEvent.AddListener(this.HandleEndHover);
        }

        private void OnDisable()
        {
            MouseStartHoveredEvent.RemoveListener(this.HandleShowingHover);
            MouseEndHoveredEvent.RemoveListener(this.HandleEndHover);
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
            Canvas.ForceUpdateCanvases();

            const float MinimumEdgeSpace = 20f;

            float minX = MinimumEdgeSpace;
            float maxX = Screen.width - MinimumEdgeSpace;
            float minY = MinimumEdgeSpace;
            float maxY = Screen.height - MinimumEdgeSpace;

            float leftmost = float.MaxValue;
            float rightmost = float.MinValue;
            float topmost = float.MinValue;
            float bottommost = float.MaxValue;

            foreach (RectTransform curTransform in this.CanvasSpaceHoverUX.GetAllRectTransforms())
            {
                leftmost = Mathf.Min(leftmost, curTransform.rect.xMin + curTransform.position.x);
                rightmost = Mathf.Max(rightmost, curTransform.rect.xMax + curTransform.position.x);
                topmost = Mathf.Min(topmost, curTransform.rect.yMax + curTransform.position.y);
                bottommost = Mathf.Max(bottommost, curTransform.rect.yMin + curTransform.position.y);
            }

            if (leftmost < minX)
            {
                this.CanvasSpaceHoverUX.OwnTransform.position += Vector3.right * (minX - leftmost);
            }

            if (rightmost > maxX)
            {
                this.CanvasSpaceHoverUX.OwnTransform.position += Vector3.left * (rightmost - maxX);
            }

            if (topmost > maxY)
            {
                this.CanvasSpaceHoverUX.OwnTransform.position += Vector3.down * (topmost - minY);
            }

            if (bottommost < minY)
            {
                this.CanvasSpaceHoverUX.OwnTransform.position += Vector3.up * (minY - bottommost);
            }
        }
    }
}