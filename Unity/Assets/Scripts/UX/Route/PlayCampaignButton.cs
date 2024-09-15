namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayCampaignButton : MonoBehaviour
    {
        public RouteImport RepresentingRoute;
        private CampaignChooserUX ChooserUX;

        public TMPro.TMP_Text Label;

        public void SetFromRoute(RouteImport route, CampaignChooserUX chooserUX)
        {
            this.RepresentingRoute = route;
            this.ChooserUX = chooserUX;
            this.Label.text = route.RouteName;
        }

        public void RouteChosen()
        {
            this.ChooserUX.RouteChosen(this.RepresentingRoute);
        }
    }
}