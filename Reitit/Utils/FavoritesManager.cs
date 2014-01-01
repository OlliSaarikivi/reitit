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
    [KnownType(typeof(Stop))]
    [KnownType(typeof(Address))]
    [KnownType(typeof(Poi))]
    public class FavoriteLocation
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int IconIndex { get; set; }
        [DataMember]
        public string LocationName { get; set; }
        [DataMember]
        public ReittiCoordinate Coordinate;
    }

    public class FavoritesManager
    {
        private static Setting<Dictionary<int, FavoriteLocation>> LocationsSetting
            = new Setting<Dictionary<int,FavoriteLocation>>("FavoriteLocations", () => new Dictionary<int,FavoriteLocation>());
        private static Setting<int> NextId = new Setting<int>("NextFavoriteId", 0);

        public FavoriteLocation this[int id]
        {
            get
            {
                return _locations[id];
            }
        }

        public ObservableCollection<FavoriteLocation> SortedLocations { get { return _sortedLocations; } }
        private ObservableCollection<FavoriteLocation> _sortedLocations = new ObservableCollection<FavoriteLocation>();

        private Dictionary<int, FavoriteLocation> _locations;

        public FavoritesManager()
        {
            _locations = LocationsSetting.Value;

            SortedLocations.AddRange(from loc in _locations.Values
                                     orderby loc.Name
                                     select loc);
        }

        public int Add(FavoriteLocation location)
        {
            int id = GetUniqueId();
            _locations[id] = location;
            int insertIndex = _sortedLocations.IndexOf(_sortedLocations.FirstOrDefault(loc => loc.Name.CompareTo(location.Name) >= 0));
            insertIndex = insertIndex == -1 ? _sortedLocations.Count : insertIndex;
            _sortedLocations.Insert(insertIndex, location);
            return id;
        }

        public void Remove(int id)
        {
            _sortedLocations.Remove(_locations[id]);
            _locations.Remove(id);
        }

        public bool Contains(FavoriteLocation location, out int id)
        {
            foreach (var entry in _locations)
            {
                if (entry.Value == location)
                {
                    id = entry.Key;
                    return true;
                }
            }
            id = -1;
            return false;
        }

        public bool Contains(FavoriteLocation location)
        {
            int ignore;
            return Contains(location, out ignore);
        }

        internal int GetUniqueId()
        {
            return NextId.Value++;
        }
    }
}
