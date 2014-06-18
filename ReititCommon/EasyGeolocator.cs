using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Geolocation;

namespace ReititCommon
{
    public delegate void StatusChangedHandler(PositionStatus status);
    public delegate void PositionChangedHandler(Geocoordinate position);

    public class GeolocationDisabledException : Exception { }

    public class EasyGeolocator
    {
        Geolocator _locator;
        event StatusChangedHandler _statusEvent;
        event PositionChangedHandler _positionEvent;
        Geocoordinate _cachedCoordinate;

        public PositionAccuracy DesiredAccuracy
        {
            get { return _locator.DesiredAccuracy; }
            set { _locator.DesiredAccuracy = value; }
        }

        public uint? DesiredAccuracyInMeters
        {
            get { return _locator.DesiredAccuracyInMeters; }
            set { _locator.DesiredAccuracyInMeters = value; }
        }

        public PositionStatus LocationStatus
        {
            get { return _locator.LocationStatus; }
        }

        public double MovementThreshold
        {
            get { return _locator.MovementThreshold; }
            set { _locator.MovementThreshold = value; }
        }

        public uint ReportInterval
        {
            get { return _locator.ReportInterval; }
            set { _locator.ReportInterval = value; }
        }

        public event StatusChangedHandler StatusChanged
        {
            add
            {
                lock (_statusEvent)
                {
                    bool start = _statusEvent == null;
                    _statusEvent += value;
                    if (start)
                    {
                        _locator.StatusChanged += _locator_StatusChanged;
                    }
                }
            }
            remove
            {
                lock (_statusEvent)
                {
                    _statusEvent -= value;
                    if (_statusEvent == null)
                    {
                        _locator.StatusChanged -= _locator_StatusChanged;
                    }
                }
            }
        }

        public event PositionChangedHandler PositionChanged
        {
            add
            {
                lock (_positionEvent)
                {
                    bool start = _positionEvent == null;
                    _positionEvent += value;
                    if (start)
                    {
                        _locator.PositionChanged += _locator_PositionChanged;
                    }
                }
            }
            remove
            {
                lock (_statusEvent)
                {
                    _positionEvent -= value;
                    if (_positionEvent == null)
                    {
                        _locator.PositionChanged -= _locator_PositionChanged;
                    }
                }
            }
        }

        public EasyGeolocator()
        {
            _locator = new Geolocator();
        }

        public async Task<Geocoordinate> GetGeopositionAsync(TimeSpan? maximumAge = null, TimeSpan? timeout = null)
        {
            if (maximumAge.HasValue && _cachedCoordinate.Timestamp > DateTimeOffset.Now - maximumAge)
            {
                return _cachedCoordinate;
            }

            if (_locator.LocationStatus == PositionStatus.Disabled)
            {
                throw new GeolocationDisabledException();
            }

            var coordinateSource = new TaskCompletionSource<Geocoordinate>();

            StatusChangedHandler statusHandler = null;
            PositionChangedHandler positionHandler = null;
            statusHandler = s =>
            {
                switch (s)
                {
                    case PositionStatus.Disabled:
                        StatusChanged -= statusHandler;
                        PositionChanged -= positionHandler;
                        coordinateSource.SetException(new GeolocationDisabledException());
                        break;
                }
            };
            positionHandler = p =>
            {
                StatusChanged -= statusHandler;
                PositionChanged -= positionHandler;
                coordinateSource.SetResult(p);
            };
            StatusChanged += statusHandler;
            PositionChanged += positionHandler;

            if (timeout.HasValue)
            {
                var firstReady = await Task.WhenAny(Task.Delay(timeout.Value), coordinateSource.Task);
                if (firstReady != coordinateSource.Task)
                {
                    StatusChanged -= statusHandler;
                    PositionChanged -= positionHandler;
                    return null;
                }
            }
            var coordinate = await coordinateSource.Task;
            StatusChanged -= statusHandler;
            PositionChanged -= positionHandler;
            return coordinate;
        }

        void _locator_StatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var statusEvent = _statusEvent;
                if (statusEvent != null)
                {
                    statusEvent(e.Status);
                }
            });
        }

        void _locator_PositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var positionEvent = _positionEvent;
                if (positionEvent != null)
                {
                    positionEvent(e.Position.Coordinate);
                }
            });
        }
    }
}
