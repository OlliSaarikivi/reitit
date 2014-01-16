﻿using Reitit.Resources;
using ReittiAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Reitit
{
    [DataContract]
    public class RouteLoader : ExtendedObservableObject
    {
        private Task _initialLoad, _loadPrevious, _loadNext;
        private CancellationTokenSource _cancellationSource;

        public string InitialStatus
        {
            get { return _initialStatus; }
            set { Set(() => InitialStatus, ref _initialStatus, value); }
        }
        private string _initialStatus;

        public string PreviousStatus
        {
            get { return _previousStatus; }
            set { Set(() => PreviousStatus, ref _previousStatus, value); }
        }
        private string _previousStatus;

        public string NextStatus
        {
            get { return _nextStatus; }
            set { Set(() => NextStatus, ref _nextStatus, value); }
        }
        private string _nextStatus;

        public string From { get { return _parameters.From.Name; } }
        public string To { get { return _parameters.To.Name; } }

        [DataMember]
        public ReittiCoordinate _cachedFrom, _cachedTo;

        [DataMember]
        public RouteSearchParameters _parameters;

        public ObservableCollection<CompoundRoute> LoadedRoutes { get { return _loadedRoutes; } }
        [DataMember]
        public ObservableCollection<CompoundRoute> _loadedRoutes = new ObservableCollection<CompoundRoute>();

        public RouteLoader(RouteSearchParameters parameters)
        {
            _parameters = parameters;
            Initialize();
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            Initialize();
        }

        private void Initialize()
        {
            _cancellationSource = new CancellationTokenSource();
            if (LoadedRoutes.Count == 0)
            {
                _initialLoad = Task.Factory.StartNew(async () =>
                {
                    await LoadInitial();
                    _initialLoad = null;
                }).Unwrap();
            }
        }

        private async Task LoadInitial()
        {
            try
            {
                var routes = await LoadRoutes(_cancellationSource.Token, s => 
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => InitialStatus = s);
                });
                Deployment.Current.Dispatcher.BeginInvoke(() => LoadedRoutes.AddRange(routes));
            }
            catch (OperationCanceledException)
            { }
        }

        public async Task LoadPrevious()
        {
            try
            {
                if (_loadPrevious != null)
                {
                    await _loadPrevious;
                }
                else
                {
                    _loadPrevious = DoLoadPrevious();
                    await _loadPrevious;
                    _loadPrevious = null;
                }
            }
            catch (OperationCanceledException)
            { }
        }

        private async Task DoLoadPrevious()
        {
            var initialLoad = _initialLoad;
            if (initialLoad != null)
            {
                await initialLoad;
            }

            var firstArrival = LoadedRoutes[0].Routes.LastElement().Legs.LastElement().Locs.LastElement().ArrTime;

            var routes = await LoadRoutes(_cancellationSource.Token, s => PreviousStatus = s, firstArrival - TimeSpan.FromMinutes(1), "arrival");
            for (int i = routes.Count - 1; i >= 0; --i)
            {
                LoadedRoutes.Insert(0, routes[i]);
            }
        }

        public async Task LoadNext()
        {
            try
            {
                if (_loadNext != null)
                {
                    await _loadNext;
                }
                else
                {
                    _loadNext = DoLoadNext();
                    await _loadNext;
                    _loadNext = null;
                }
            }
            catch (OperationCanceledException)
            { }
        }

        private async Task DoLoadNext()
        {
            var initialLoad = _initialLoad;
            if (initialLoad != null)
            {
                await initialLoad;
            }

            var lastDeparture = LoadedRoutes.LastElement().Routes[0].Legs[0].Locs[0].DepTime;

            var routes = await LoadRoutes(_cancellationSource.Token, s => NextStatus = s, lastDeparture + TimeSpan.FromMinutes(1), "departure");
            LoadedRoutes.AddRange(routes);
        }

        private async Task<List<CompoundRoute>> LoadRoutes(CancellationToken token, Action<string> updateStatus, DateTime? dateTime = null, string timetype = null)
        {
            if (_cachedFrom == null)
            {
                updateStatus(AppResources.RouteLoaderLocatingStatus);
                _cachedFrom = await _parameters.From.GetCoordinates();
            }
            if (_cachedTo == null)
            {
                updateStatus(AppResources.RouteLoaderLocatingStatus);
                _cachedTo = await _parameters.To.GetCoordinates();
            }
            updateStatus(AppResources.RouteLoaderLoadingStatus);
            try
            {
                var routes = await App.Current.ReittiClient.RouteAsync(
                    _cachedFrom,
                    _cachedTo,
                    dateTime: dateTime ?? _parameters.DateTime,
                    timetype: timetype ?? _parameters.Timetype,
                    transportTypes: _parameters.TransportTypes,
                    optimize: _parameters.Optimize,
                    changeMargin: _parameters.ChangeMargin,
                    walkSpeed: _parameters.WalkSpeed,
                    detail: "full",
                    show: 5,
                    cancellationToken: token);
                token.ThrowIfCancellationRequested();
                return routes;
            }
            finally
            {
                updateStatus(null);
            }
        }
    }

    [DataContract]
    public class RouteSearchParameters
    {
        [DataMember]
        public IPickerLocation From;
        [DataMember]
        public IPickerLocation To;
        [DataMember]
        public DateTime? DateTime;
        [DataMember]
        public string Timetype;
        [DataMember]
        public List<string> TransportTypes;
        [DataMember]
        public string Optimize;
        [DataMember]
        public int? ChangeMargin;
        [DataMember]
        public int? WalkSpeed;

        public RouteSearchParameters()
        {
            TransportTypes = new List<string>();
        }
    }
}
