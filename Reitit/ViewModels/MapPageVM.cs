using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Reitit
{
    [DataContract]
    class MapPageVM : ViewModelBase
    {
        public bool ContentMaximized
        {
            get { return _contentMaximized; }
            set
            {
                Set(() => ContentMaximized, ref _contentMaximized, value);
                StopTracking();
            }
        }
        [DataMember]
        public bool _contentMaximized = true;

        public double ContentMinimizedOffset
        {
            get { return _contentMinimizedOffset; }
            set { Set(() => ContentMinimizedOffset, ref _contentMinimizedOffset, value); }
        }
        [DataMember]
        public double _contentMinimizedOffset;

        public double ContentMinimizedHeight
        {
            get { return _contentMinimizedHeight; }
            set { Set(() => ContentMinimizedHeight, ref _contentMinimizedHeight, value); }
        }
        [DataMember]
        public double _contentMinimizedHeight;

        public ReittiCoordinate MyLocation
        {
            get { return _myLocation; }
            set
            {
                Set(() => MyLocation, ref _myLocation, value);

                if (_myLocation != null && _trackMyLocation)
                {
                    Messenger.Default.Send(new MapSetViewMessage { Center = _myLocation, });

                    /* Disable after first time for now because I could not find a way to detect when 
                     * the user has panned the map vs when I have changed the center myself. */
                    _trackMyLocation = false;
                }
            }
        }
        [DataMember]
        public ReittiCoordinate _myLocation;

        public ReittiCoordinate MapCenter
        {
            get { return _mapCenter; }
            set { Set(() => MapCenter, ref _mapCenter, value); }
        }
        [DataMember]
        public ReittiCoordinate _mapCenter = Utils.DefaultViewCoordinate;

        public double MapZoomLevel
        {
            get { return _mapZoomLevel; }
            set { Set(() => MapZoomLevel, ref _mapZoomLevel, value); }
        }
        [DataMember]
        public double _mapZoomLevel = Utils.DefaultViewZoom;

        [DataMember]
        public bool _showMyLocationExplicit = false;

        public bool ShowMyLocationImplicit
        {
            get { return _showMyLocationImplicit; }
            set { Set(() => ShowMyLocationImplicit, ref _showMyLocationImplicit, value); }
        }
        [DataMember]
        public bool _showMyLocationImplicit = false;

        [DataMember]
        public bool _trackMyLocation = false;

        [DataMember]
        private bool _locating = false;

        private TrayStatus _locatingStatus;

        protected override void Initialize()
        {
            _locatingStatus = new TrayStatus();
            if (_locating)
            {
                _locating = false;
                StartGPS();
            }
            Messenger.Default.Register<ResumingMessage>(this, m =>
            {
                if (_locating)
                {
                    _locating = false;
                    StartGPS();
                }
            });
            Messenger.Default.Register<SuspendingMessage>(this, m =>
            {
                if (_locating)
                {
                    StopGPS();
                    _locating = true;
                }
            });
        }

        public RelayCommand ShowMyLocationExplicitCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _showMyLocationExplicit = true;
                    _trackMyLocation = true;
                    StartGPS();
                    if (MyLocation != null)
                    {
                        Messenger.Default.Send(new MapSetViewMessage { Center = _myLocation, });
                    }
                });
            }
        }

        public void StopTracking()
        {
            _trackMyLocation = false;
        }

        public async Task ClearShowMyLocation()
        {
            _showMyLocationExplicit = false;
            ShowMyLocationImplicit = false;
            await UpdateGPS();
        }

        public async Task SetShowMyLocationImplicit()
        {
            ShowMyLocationImplicit = true;
            await UpdateGPS();
        }

        public void ClearShowMyLocationImplicit()
        {
            ShowMyLocationImplicit = false;
            Utils.OnCoreDispatcher(async () =>
            {
                await UpdateGPS();
            });
        }

        async Task UpdateGPS()
        {
            if (_showMyLocationExplicit || ShowMyLocationImplicit)
            {
                StartGPS();
            }
            else
            {
                StopTracking();
                await StopGPS();
            }
        }

        void StartGPS()
        {
            if (!_locating)
            {
                _locating = true;
                App.Current.Geolocator.StatusChanged += Geolocator_StatusChanged;
                App.Current.Geolocator.PositionChanged += Geolocator_PositionChanged;
            }
        }

        async Task StopGPS()
        {
            if (_locating)
            {
                App.Current.Geolocator.PositionChanged -= Geolocator_PositionChanged;
                App.Current.Geolocator.StatusChanged -= Geolocator_StatusChanged;
                MyLocation = null;
                await _locatingStatus.Remove();
                _locating = false;
            }
        }

        private async void Geolocator_StatusChanged(PositionStatus status)
        {
            await UpdateForGeolocatorStatus(status);
        }

        private async Task UpdateForGeolocatorStatus(PositionStatus status)
        {
            switch (status)
            {
                case PositionStatus.Disabled:
                    MyLocation = null;
                    await _locatingStatus.Remove();
                    if (_showMyLocationExplicit)
                    {
                        await Utils.ShowMessageDialog(Utils.GetString("GPSDisabledMessage"), Utils.GetString("GPSDisabledMessageTitle"));
                    }
                    await ClearShowMyLocation();
                    break;
                case PositionStatus.Initializing:
                    MyLocation = null;
                    await _locatingStatus.SetText(Utils.GetString("LocatingTrayStatus"));
                    await _locatingStatus.Push();
                    break;
                case PositionStatus.NoData:
                    MyLocation = null;
                    await _locatingStatus.SetText(Utils.GetString("NoLocationDataTrayStatus"));
                    await _locatingStatus.Push();
                    break;
                case PositionStatus.NotAvailable:
                    MyLocation = null;
                    await _locatingStatus.Remove();
                    if (_showMyLocationExplicit)
                    {
                        await Utils.ShowMessageDialog(Utils.GetString("NoGPSMessage"), Utils.GetString("NoGPSMessageTitle"));
                    }
                    await ClearShowMyLocation();
                    break;
                case PositionStatus.NotInitialized:
                    MyLocation = null;
                    await _locatingStatus.Remove();
                    break;
                case PositionStatus.Ready:
                    await _locatingStatus.Remove();
                    break;
            }
        }

        void Geolocator_PositionChanged(Geocoordinate position)
        {
            MyLocation = (ReittiCoordinate)position;
        }
    }
}
