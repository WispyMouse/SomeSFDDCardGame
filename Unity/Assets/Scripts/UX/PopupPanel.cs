namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class PopupPanel : MonoBehaviour
    {
        [SerializeReference]
        private TMPro.TMP_Text Body;

        [SerializeReference]
        public HorizontalLayoutGroup LayoutGroup;

        public void SetText(string body)
        {
            this.Body.text = body;
        }
    }
}