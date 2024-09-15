namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CampaignRoute
    {
        public string RouteName => this.BasedOn.RouteName;
        public readonly List<ChoiceNode> Nodes = new List<ChoiceNode>();
        public readonly RouteImport BasedOn;

        public CampaignRoute(RunConfiguration runConfiguration, RouteImport basedOn)
        {
            this.BasedOn = basedOn;
            RandomDecider<EncounterModel> decider = new RandomDecider<EncounterModel>();

            foreach (ChoiceNodeImport node in basedOn.RouteNodes)
            {
                this.Nodes.Add(new ChoiceNode(node, decider));
            }
        }
    }
}