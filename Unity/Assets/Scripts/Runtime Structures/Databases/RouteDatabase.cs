namespace SFDDCards
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class RouteDatabase
    {
        public static List<RouteImport> AllRoutes = new List<RouteImport>();

        public static void AddRouteToDatabase(RouteImport toAdd)
        {
            AllRoutes.Add(toAdd);
        }
    }
}