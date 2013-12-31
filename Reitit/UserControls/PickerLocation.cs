using Reitit.Resources;
using ReittiAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using GalaSoft.MvvmLight;
using System.Device.Location;
using System.Windows.Media;

namespace Reitit
{
    public interface IPickerLocation
    {
        string Name { get; }
        Task<ReittiCoordinate> GetCoordinates();
    }

    public class MapLocation : ObservableObject, IPickerLocation
    {
        private ReittiCoordinate _coordinate;

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }
        private string _name = AppResources.MapLocationPlaceholderText;

        public string Detail { get { return null; } }

        public MapLocation(ReittiCoordinate coordinate)
        {
            _coordinate = coordinate;
        }
        
        public Task<ReittiCoordinate> GetCoordinates()
        {
            return Task.FromResult(_coordinate);
        }
    }

    public abstract class ReittiLocationBase : ObservableObject, IPickerLocation
    {
        public abstract string Name { get; }
        public abstract Task<ReittiCoordinate> GetCoordinates();
        public abstract string Detail { get; }
        public bool Selected
        {
            get { return _selected; }
            set { Set(() => Selected, ref _selected, value); }
        }
        private bool _selected = false;
    }

    public class ReittiLocation : ReittiLocationBase
    {
        private Location _location;

        public override string Name { get { return _location.Name; } }
        public override string Detail { get { return null; } }

        public ReittiLocation(Location location)
        {
            _location = location;
        }

        public override Task<ReittiCoordinate> GetCoordinates()
        {
            return Task.FromResult(_location.Coords);
        }
    }

    public class ReittiStopsLocation : ReittiLocationBase
    {
        private ConnectedStops _stops;

        public override string Name { get { return _stops.Name; } }
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

    public class MeLocation : IPickerLocation
    {
        public static MeLocation Instance { get { return _instance; } }
        private static MeLocation _instance = new MeLocation();

        private MeLocation() { }

        public string Name { get { return AppResources.MyLocationText; } }
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
}
