namespace SFDDCards.UX
{
    using SFDDCards.Evaluation.Actual;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using UnityEngine;

    public class EncounterDialogueButtonUX : MonoBehaviour
    {
        [SerializeReference]
        private TMPro.TMP_Text DialogueText;

        private Action InteractionAction;

        public void SetEncounterDialogue(string dialogueLabel, Action interactionAction)
        {
            this.DialogueText.text = dialogueLabel;
            this.InteractionAction = interactionAction;
        }

        public void OnClick()
        {
            this.InteractionAction.Invoke();
        }
    }
}