namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class StatusEffectUX : MonoBehaviour
    {
        public AppliedStatusEffect RepresentsEffect;

        [SerializeReference]
        private TMPro.TMP_Text StackText;

        public void SetFromEffect(AppliedStatusEffect toSet)
        {
            this.RepresentsEffect = toSet;
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
    }
}