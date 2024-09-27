namespace SFDDCards.UX
{
    using SFDDCards.Evaluation.Actual;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class PlayerChoiceController : MonoBehaviour
    {
        [SerializeReference]
        private CardBrowser CardBrowserUX;

        void OnEnable()
        {
            GlobalUpdateUX.PlayerMustMakeChoice.AddListener(HandlePlayerChoice);
        }

        void OnDisable()
        {
            GlobalUpdateUX.PlayerMustMakeChoice.RemoveListener(HandlePlayerChoice);
        }

        void HandlePlayerChoice(DeltaEntry fromDelta, PlayerChoice toHandle, Action continuationAction)
        {
            if (toHandle is PlayerChooseFromCardBrowser cardBrowser)
            {
                this.CardBrowserUX.SetFromCardBrowserChoice(fromDelta, cardBrowser, 
                    (List<Card> chosenCards) =>
                {
                    cardBrowser.SetChoice(fromDelta, chosenCards);
                    continuationAction?.Invoke();
                });
                return;
            }

            GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse player choice using the controller.", GlobalUpdateUX.LogType.RuntimeError);
        }
    }
}