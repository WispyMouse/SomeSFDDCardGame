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

            string unprocessedDialogue = this.representingModel.BasedOn.BuildEncounterDialogue(index, this.CentralGameStateControllerInstance.CurrentCampaignContext);
            this.DescriptionLabel.text = EffectDescriberDatabase.ReplaceTokensInString(unprocessedDialogue, this.CentralGameStateControllerInstance.CurrentCampaignContext);

            for (int ii = this.ButtonHolder.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.ButtonHolder.GetChild(ii).gameObject);
            }

            List<EncounterOptionImport> options = this.representingModel.BasedOn.GetOptions(index, this.CentralGameStateControllerInstance.CurrentCampaignContext);
            foreach (EncounterOptionImport option in options)
            {
                EncounterDialogueButtonUX button = Instantiate(this.DialogueButtonPF, this.ButtonHolder);
                EncounterOptionImport hungOption = option;

                string unprocessedDialogueOption = option.Dialogue;

                button.SetEncounterDialogue(EffectDescriberDatabase.ReplaceTokensInString(unprocessedDialogueOption, this.CentralGameStateControllerInstance.CurrentCampaignContext), () => this.ChooseOption(hungOption));
            }
        }

        public void ChooseOption(EncounterOptionImport option)
        {
            if (option.PossibleOutcomes == null || option.PossibleOutcomes.Count == 0)
            {
                // If there are no possible outcomes, treat it as though it was a leave
                GlobalSequenceEventHolder.PushSequenceToTop(new GameplaySequenceEvent(
                () =>
                {
                    this.GameplayUXControllerInstance.EncounterDialogueComplete(this.representingModel);
                }));
                return;
            }

            EncounterOptionOutcomeImport outcome = option.PossibleOutcomes[0];

            // Find the first requirement with matching criteria
            // It could be the first value, especially if it has no requirements
            for (int ii = 0; ii < option.PossibleOutcomes.Count; ii++)
            {
                if (this.CentralGameStateControllerInstance.CurrentCampaignContext.RequirementsAreMet(option.PossibleOutcomes[ii].Criteria))
                {
                    outcome = option.PossibleOutcomes[ii];
                    break;
                }
            }

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
                        this.GameplayUXControllerInstance.EncounterDialogueComplete(representingModel);
                    }
                    else
                    {
                        this.SetEncounterIndex(destination);
                    }
                }));
        }

        public void Close()
        {
            this.gameObject.SetActive(false);
        }
    }
}