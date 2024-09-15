namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class RouteImport
    {
        public string RouteName { get; set; }
        public List<ChoiceNodeImport> RouteNodes { get; set; }
    }
}