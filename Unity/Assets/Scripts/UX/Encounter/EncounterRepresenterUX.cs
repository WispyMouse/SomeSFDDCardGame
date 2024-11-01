namespace SFDDCards.UX
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ImportModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using UnityEngine;

    public class EncounterRepresenterUX : MonoBehaviour
    {
        [SerializeReference]
        private CentralGameStateController CentralGameStateControllerInstance;
        [SerializeReference]
        private GameplayUXController GameplayUXControllerInstance;

        [SerializeReference]
        private TMPro.TMP_Text NameLabel;
        [SerializeReference]
        private TMPro.TMP_Text DescriptionLabel;
        [SerializeReference]
        private Transform ButtonHolder;
        [SerializeReference]
        private EncounterDialogueButtonUX DialogueButtonPF;

        private string currentEncounterIndex = "intro";
        private EvaluatedEncounter representingModel = null;

        public void RepresentEncounter(EvaluatedEncounter toRepresent)
        {
            this.representingModel = toRepresent;
            this.NameLabel.text = toRepresent.BasedOn.Name;

            this.SetEncounterIndex("intro");
            this.gameObject.SetActive(true);
        }

        public void SetEncounterIndex(string index)
        {
            this.currentEncounterIndex = index;

            // TODO: Process this dialogue
            this.DescriptionLabel.text = this.representingModel.BasedOn.BuildEncounterDialogue(index);

            for (int ii = this.ButtonHolder.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.ButtonHolder.GetChild(ii).gameObject);
            }

            List<EncounterOptionImport> options = this.representingModel.BasedOn.GetOptions(index);
            foreach (EncounterOptionImport option in options)
            {
                EncounterDialogueButtonUX button = Instantiate(this.DialogueButtonPF, this.ButtonHolder);
                EncounterOptionImport hungOption = option;
                button.SetEncounterDialogue(hungOption.Dialogue, () => this.ChooseOption(hungOption));
            }
        }

        public void ChooseOption(EncounterOptionImport option)
        {
            // TODO: Determine which outcome to use
            // currently using 0 always
            EncounterOptionOutcomeImport outcome = option.PossibleOutcomes[0];

            GamestateDelta delta = ScriptTokenEvaluator.GetDeltaFromTokens(outcome.Effect,
                this.CentralGameStateControllerInstance.CurrentCampaignContext,
                null,
                this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer,
                this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer);

            string destination = delta.GetEncounterDestination();

            GlobalSequenceEventHolder.PushSequenceToTop(new GameplaySequenceEvent(
                () =>
                {
                    this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer.ApplyDelta(
                           this.CentralGameStateControllerInstance.CurrentCampaignContext,
                           null,
                           delta.DeltaEntries);

                    if (string.IsNullOrEmpty(destination))
                    {
                        this.GameplayUXControllerInstance.EncounterDialogueComplete();
                    }
                    else
                    {
                        this.SetEncounterIndex(destination);
                    }
                }));
        }
    }
}