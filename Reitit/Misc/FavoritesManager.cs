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

    [DataContract]
    public class FavoritesManagerSettings
    {
        [DataMember]
        public Dictionary<int, FavoriteLocation> FavoriteLocations = new Dictionary<int, FavoriteLocation>();
        [DataMember]
        public int NextFavoriteId = 0;
    }

    public class FavoritesManager
    {
        public FavoriteLocation this[int id]
        {
            get
            {
                return _locations[id];
            }
        }

        public ObservableCollection<FavoriteLocation> SortedLocations { get; private set; }

        private Dictionary<int, FavoriteLocation> _locations;

        public FavoritesManager()
        {
            _locations = App.Current.Settings.Favorites.FavoriteLocations;

            SortedLocations = new ObservableCollection<FavoriteLocation>(from loc in _locations.Values
                                     orderby loc.Name
                                     select loc);
        }

        public int Add(FavoriteLocation location)
        {
            int id = GetUniqueId();
            _locations[id] = location;
            int insertIndex = SortedLocations.IndexOf(SortedLocations.FirstOrDefault(loc => loc.Name.CompareTo(location.Name) >= 0));
            insertIndex = insertIndex == -1 ? SortedLocations.Count : insertIndex;
            SortedLocations.Insert(insertIndex, location);
            return id;
        }

        public void Remove(int id)
        {
            SortedLocations.Remove(_locations[id]);
            _locations.Remove(id);
        }

        public IEnumerable<FavoriteLocation> LocationsWithPrefix(string prefix)
        {
            foreach (var favorite in SortedLocations)
            {
                if (favorite.Name.StartsWith(prefix))
                {
                    yield return favorite;
                }
            }
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
            return App.Current.Settings.Favorites.NextFavoriteId++;
        }
    }
}
