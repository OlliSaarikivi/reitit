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
    public class FavoriteLocation : ExtendedObservableObject
    {
        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }
        [DataMember]
        public string _name;
        public string IconName
        {
            get { return _iconName; }
            set { Set(() => IconName, ref _iconName, value); }
        }
        [DataMember]
        public string _iconName;
        public string LocationName
        {
            get { return _locationName; }
            set { Set(() => LocationName, ref _locationName, value); }
        }
        [DataMember]
        public string _locationName;
        public ReittiCoordinate Coordinate
        {
            get { return _coordinate; }
            set { Set(() => Coordinate, ref _coordinate, value); }
        }
        [DataMember]
        public ReittiCoordinate _coordinate;
        public int Id
        {
            get { return _id; }
            set { Set(() => Id, ref _id, value); }
        }
        [DataMember]
        public int _id;

        public FavoriteLocation()
        {
            IconName = Utils.DefaultFavIcon;
        }
    }

    [DataContract]
    public class FavoritesManagerSettings
    {
        [DataMember]
        public ObservableCollection<FavoriteLocation> FavoriteLocations = new ObservableCollection<FavoriteLocation>();
        [DataMember]
        public int NextFavoriteId = 0;
    }

    public class FavoritesManager
    {
        public ObservableCollection<FavoriteLocation> SortedLocations { get; private set; }

        public FavoritesManager()
        {
            SortedLocations = App.Current.Settings.Favorites.FavoriteLocations;
            SortedLocations.CollectionChanged += (s, e) => { App.Current.LocationPickerFlyout.Clear(); };
        }

        public int Add(FavoriteLocation location, int index = -1)
        {
            int id = GetUniqueId();
            location.Id = id;
            if (index >= 0 && index < SortedLocations.Count)
            {
                SortedLocations.Insert(index, location);
            }
            else
            {
                SortedLocations.Add(location);
            }
            return id;
        }

        public int Remove(int id)
        {
            var result = SortedLocations.FirstOrDefault(fav => fav.Id == id);
            if (result != null)
            {
                var index = SortedLocations.IndexOf(result);
                SortedLocations.RemoveAt(index);
                return index;
            }
            return -1;
        }

        public void ReplaceOrAdd(int id, FavoriteLocation newLocation)
        {
            var index = Remove(id);
            Add(newLocation, index);
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

        public bool Contains(FavoriteLocation search, out int id)
        {
            foreach (var location in SortedLocations)
            {
                if (location == search)
                {
                    id = location.Id;
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
