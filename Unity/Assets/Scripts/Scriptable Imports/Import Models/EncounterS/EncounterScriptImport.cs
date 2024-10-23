namespace SFDDCards.ImportModels
{
    using System;
    using System.Collections.Generic;

    [Serializable]

    public class EncounterScriptImport
    {
        public string Id;
        public List<EncounterDialogueSegmentImport> DialogueParts;
        public List<EncounterOptionImport> Options;
    }
}