namespace SFDDCards.UX
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class StatusEffectUX : MonoBehaviour, IMouseHoverListener
    {
        public AppliedStatusEffect RepresentsEffect;

        [SerializeReference]
        private TMPro.TMP_Text StackText;

        [SerializeReference]
        private Image Renderer;

        public Transform GetTransform()
        {
            return this.GetComponent<RectTransform>();
        }

        public void SetFromEffect(AppliedStatusEffect toSet)
        {
            this.RepresentsEffect = toSet;
            this.Renderer.sprite = toSet.BasedOnStatusEffect.Sprite;
        }

        public void SetStacks(int toStack)
        {
            if (toStack <= 1)
            {
                this.StackText.gameObject.SetActive(false);
                return;
            }

            this.StackText.gameObject.SetActive(true);
            this.StackText.text = toStack.ToString();
        }

        public bool TryGetCard(out Card toShow)
        {
            toShow = null;
            return false;
        }

        public bool TryGetStatusEffect(out AppliedStatusEffect toShow)
        {
            toShow = this.RepresentsEffect;
            return true;
        }

        public virtual void MouseEnterStartHover()
        {
            MouseHoverShowerController.MouseStartHoveredEvent.Invoke(this);
        }

        public virtual void MouseExitStopHover()
        {
            MouseHoverShowerController.MouseEndHoveredEvent.Invoke(this);
        }

        private void OnDisable()
        {
            MouseHoverShowerController.MouseEndHoveredEvent.Invoke(this);
        }
    }
}