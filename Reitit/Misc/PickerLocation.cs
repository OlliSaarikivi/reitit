using Reitit.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using GalaSoft.MvvmLight;
using System.Runtime.Serialization;
using Windows.Services.Maps;

namespace Reitit
{
    public interface IPickerLocation
    {
        string Name { get; }
        string Detail { get; }
        Task<ReittiCoordinate> GetCoordinates();
    }

    [DataContract]
    public class MeLocation : IPickerLocation
    {
        public static MeLocation Instance { get { return _instance; } }
        private static MeLocation _instance = new MeLocation();

        private MeLocation() { }

        public string Name { get { return Utils.GetString("MyLocationText"); } }
        public string Detail { get { return null; } }
        public async Task<ReittiCoordinate> GetCoordinates()
        {
            var locator = new Geolocator
            {
                DesiredAccuracy = PositionAccuracy.High
            };
            return (ReittiCoordinate)(await locator.GetGeopositionAsync(TimeSpan.Zero, TimeSpan.FromSeconds(15))).Coordinate;
        }
    }

    [DataContract]
    public class MapLocation : ExtendedObservableObject, IPickerLocation
    {
        public ReittiCoordinate Coordinate
        {
            get { return _coordinate; }
            set { Set(() => Coordinate, ref _coordinate, value); }
        }
        [DataMember]
        public ReittiCoordinate _coordinate;

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }
        [DataMember]
        public string _name = Utils.GetString("MapLocationPlaceholderText");

        public string Detail { get { return null; } }

        public Task<ReittiCoordinate> GetCoordinates()
        {
            return Task.FromResult(Coordinate);
        }

        public async Task UpdateNameFromReverseGeocode()
        {
            try
            {
                var result = await App.Current.ReittiClient.ReverseGeocodeAsync(Coordinate, limit: 1);
                string nearestName = null;
                double nearestDistance = double.MaxValue;
                foreach (var location in result.GetAllLocations())
                {
                    double distance = Coordinate.SquaredDistanceTo(location.Coords);
                    if (distance < nearestDistance)
                    {
                        nearestName = location.Name;
                        nearestDistance = distance;
                    }
                }
                if (nearestName != null)
                {
                    Name = nearestName;
                }
            }
            catch (ReittiAPIException e) {
                return;
            }
        }
    }

    [DataContract]
    public abstract class SelectableLocation : ExtendedObservableObject, IPickerLocation
    {
        public abstract string Name { get; }
        public abstract string Detail { get; }
        public abstract Task<ReittiCoordinate> GetCoordinates();
        public abstract bool Selected { get; set; }
    }

    [DataContract]
    public class FavoritePickerLocation : SelectableLocation
    {
        [DataMember]
        public FavoriteLocation _location;

        public override string Name { get { return _location.Name; } }
        public override string Detail { get { return null; } }

        public override bool Selected
        {
            get { return _selected; }
            set { Set(() => Selected, ref _selected, value); }
        }
        [DataMember]
        public bool _selected = false;

        public FavoritePickerLocation(FavoriteLocation location)
        {
            _location = location;
        }

        public override Task<ReittiCoordinate> GetCoordinates()
        {
            return Task.FromResult(_location.Coordinate);
        }
    }

    [DataContract]
    public class RecentPickerLocation : SelectableLocation
    {
        [DataMember]
        public RecentLocation _location;

        public override string Name { get { return _location.Name; } }
        public override string Detail { get { return _location.Detail; } }

        public override bool Selected
        {
            get { return _selected; }
            set { Set(() => Selected, ref _selected, value); }
        }
        [DataMember]
        public bool _selected = false;

        public RecentPickerLocation(RecentLocation location)
        {
            _location = location;
        }

        public override Task<ReittiCoordinate> GetCoordinates()
        {
            return Task.FromResult(_location.Coordinate);
        }
    }

    [DataContract]
    public abstract class ReittiLocationBase : SelectableLocation
    {
        public abstract string LongName { get; }
        public override bool Selected
        {
            get { return _selected; }
            set { Set(() => Selected, ref _selected, value); }
        }
        [DataMember]
        public bool _selected = false;
    }

    [KnownType(typeof(Poi))]
    [KnownType(typeof(Address))]
    [KnownType(typeof(Stop))]
    [DataContract]
    public class ReittiLocation : ReittiLocationBase
    {
        [DataMember]
        public Location _location;

        public override string Name { get { return _location.Name; } }
        public override string LongName { get { return _location.LongName; } }
        public override string Detail { get { return Utils.GetPreferredName(_location.CitiesByLang); } }

        public ReittiLocation(Location location)
        {
            _location = location;
        }

        public override Task<ReittiCoordinate> GetCoordinates()
        {
            return Task.FromResult(_location.Coords);
        }
    }

    [DataContract]
    public class ReittiStopsLocation : ReittiLocationBase
    {
        [DataMember]
        public ConnectedStops _stops;

        public override string Name { get { return _stops.Name; } }
        public override string LongName { get { return _stops.Name; } }
        public override string Detail { get { return _stops.DisplayCode; } }

        public ReittiStopsLocation(ConnectedStops stops)
        {
            _stops = stops;
        }

        public override Task<ReittiCoordinate> GetCoordinates()
        {
            return Task.FromResult(_stops.Center);
        }
    }
}
