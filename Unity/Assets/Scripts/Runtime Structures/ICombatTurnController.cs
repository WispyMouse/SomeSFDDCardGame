namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public interface ICombatTurnController
    {
        public CampaignContext ForCampaign { get; }

        public void HandleSequenceEventWithAnimation(GameplaySequenceEvent sequenceEvent);
    }
}