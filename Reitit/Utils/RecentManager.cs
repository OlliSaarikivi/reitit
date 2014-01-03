using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using ReittiAPI;
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
        public ReittiCoordinate Coordinate;
    }

    public class RecentManager
    {
        public const int RecentLocationsSize = 20;

        private static Setting<ObservableCollection<RecentLocation>> LocationsSetting
            = new Setting<ObservableCollection<RecentLocation>>("RecentLocations", () => new ObservableCollection<RecentLocation>());

        public ObservableCollection<RecentLocation> Locations { get; private set; }

        public RecentManager()
        {
            Locations = LocationsSetting.Value;
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
    }
}
