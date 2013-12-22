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

namespace Reitit
{
    public interface IPickerLocation
    {
        string DisplayName { get; }
        Task<ReittiCoordinate> GetCoordinates();
    }

    class MapLocation : ObservableObject, IPickerLocation
    {
        private ReittiCoordinate _coordinate;

        public string DisplayName
        {
            get { return _displayName; }
            set { Set(() => DisplayName, ref _displayName, value); }
        }
        private string _displayName = AppResources.MapLocationPlaceholderText;

        public MapLocation(ReittiCoordinate coordinate)
        {
            _coordinate = coordinate;
        }
        
        public Task<ReittiCoordinate> GetCoordinates()
        {
            return Task.FromResult(_coordinate);
        }
    }

    class ReittiLocation : IPickerLocation
    {
        public Location Location { get; private set; }

        public string DisplayName { get { return Location.Name; } }

        public ReittiLocation(Location location)
        {
            Location = location;
        }

        public Task<ReittiCoordinate> GetCoordinates()
        {
            return Task.FromResult(Location.Coords);
        }
    }

    class MeLocation : IPickerLocation
    {
        public string DisplayName { get { return AppResources.MyLocationText; } }
        public async Task<ReittiCoordinate> GetCoordinates()
        {
            var locator = new Geolocator
            {
                DesiredAccuracy = PositionAccuracy.High
            };
            return (await locator.GetGeopositionAsync(TimeSpan.Zero, TimeSpan.FromSeconds(15))).ToReittiCoordinate();
        }
    }
}
