using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Reitit.API;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace Reitit
{
    [DataContract]
    public class RecentLocation
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Detail { get; set; }
        [DataMember]
        public ReittiCoordinate Coordinate;
    }

    public class RecentManager
    {
        public const int RecentLocationsSize = 20;

        public ObservableCollection<RecentLocation> Locations { get; private set; }

        public RecentManager()
        {
            Locations = App.Current.Settings.RecentLocations;
        }

        public void Add(RecentLocation location)
        {
            for (int i = Locations.Count - 1; i >= 0; --i)
            {
                if (Locations[i].Name == location.Name)
                {
                    Locations.RemoveAt(i);
                }
            }
            Locations.Insert(0, location);
            while (Locations.Count > RecentLocationsSize)
            {
                Locations.RemoveAt(Locations.Count - 1);
            }
        }

        public IEnumerable<RecentLocation> LocationsWithPrefix(string prefix)
        {
            foreach (var recent in Locations)
            {
                if (recent.Name.StartsWith(prefix))
                {
                    yield return recent;
                }
            }
        }
    }
}
