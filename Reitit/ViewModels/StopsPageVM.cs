using Reitit.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reitit
{
    [DataContract]
    class DepartureVM : ExtendedObservableObject
    {
        [DataMember]
        public Line Line;

        [DataMember]
        public ConnectedLines ConnectedLines;

        public DateTime Time
        {
            get { return _time; }
            set { Set(() => Time, ref _time, value); }
        }
        [DataMember]
        public DateTime _time;

        public string Text
        {
            get { return _text; }
            set { Set(() => Text, ref _text, value); }
        }
        [DataMember]
        public string _text;

        public string Detail
        {
            get { return _detail; }
            set { Set(() => Detail, ref _detail, value); }
        }
        [DataMember]
        public string _detail;
    }

    [DataContract]
    class StopPivotItemVM : ExtendedObservableObject
    {
        private CancellationTokenSource _cancellationSource;

        [DataMember]
        public ConnectedStops Stops;

        public string Code
        {
            get { return _code; }
            set { Set(() => Code, ref _code, value); }
        }
        [DataMember]
        public string _code;

        public bool Loading
        {
            get { return _loading; }
            set { Set(() => Loading, ref _loading, value); }
        }
        [DataMember]
        public bool _loading;

        public bool HasLoaded
        {
            get { return _hasLoaded; }
            set { Set(() => HasLoaded, ref _hasLoaded, value); }
        }
        [DataMember]
        public bool _hasLoaded;

        public string NoDeparturesExplanation
        {
            get { return _noDeparturesExplanation; }
            set { Set(() => NoDeparturesExplanation, ref _noDeparturesExplanation, value); }
        }
        [DataMember]
        public string _noDeparturesExplanation;

        public DerivedProperty<bool> ShowExplanationProperty;
        public bool ShowExplanation { get { return ShowExplanationProperty.Get(); } }

        public ObservableCollection<DepartureVM> Departures { get { return _departures; } }
        [DataMember]
        public ObservableCollection<DepartureVM> _departures = new ObservableCollection<DepartureVM>();

        public StopPivotItemVM(ConnectedStops stops)
        {
            ShowExplanationProperty = CreateDerivedProperty(() => ShowExplanation,
                () => HasLoaded && Departures.Count == 0);

            Stops = stops;
            Code = stops.DisplayCode;
        }

        public async Task UpdateDepartures()
        {
            if (_cancellationSource != null)
            {
                _cancellationSource.Cancel();
            }
            _cancellationSource = new CancellationTokenSource();
            try
            {
                try
                {
                    Loading = true;
                    Departures.Clear();
                    HasLoaded = false;
                    NoDeparturesExplanation = Utils.GetString("NoDepartures6Hours");

                    var client = App.Current.ReittiClient;
                    var tasks = from stop in Stops.Locations
                                select client.StopInformationAsync(stop, timeLimit: TimeSpan.FromMinutes(360), depLimit: 20, cancellationToken: _cancellationSource.Token);
                    await Task.WhenAll(tasks);
                    _cancellationSource.Token.ThrowIfCancellationRequested();
                    var allDepartures = new List<DepartureVM>();
                    foreach (var stop in Stops.Locations)
                    {
                        foreach (var departure in stop.Departures)
                        {
                            var connectedLines = stop.ConnectedLines.FirstOrDefault(x => x.Lines.Contains(departure.Line));
                            var departureVM = new DepartureVM
                            {
                                Line = departure.Line,
                                ConnectedLines = connectedLines, // May be null
                                Time = departure.Time,
                                Text = departure.Line.ShortName,
                                Detail = departure.Line.LineEnd,
                            };
                            allDepartures.Add(departureVM);
                        }
                    }
                    allDepartures.Sort((d1, d2) => d1.Time.CompareTo(d2.Time));

                    Departures.AddRange(allDepartures);
                    HasLoaded = true;
                    Loading = false;
                }
                catch (ReittiAPIException)
                {
                    _cancellationSource.Token.ThrowIfCancellationRequested();
                    Loading = false;
                    NoDeparturesExplanation = Utils.GetString("NoDeparturesError");
                }
            }
            catch (OperationCanceledException) { }
        }
    }

    [DataContract]
    class StopsPageVM : ViewModelBase
    {
        public ObservableCollection<StopPivotItemVM> StopPivotItems { get { return _stopPivotItems; } }
        [DataMember]
        public ObservableCollection<StopPivotItemVM> _stopPivotItems = new ObservableCollection<StopPivotItemVM>();

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }
        [DataMember]
        public string _name;

        public StopsPageVM(IEnumerable<ConnectedStops> connectedStops)
            : base(false)
        {
            var someStop = connectedStops.FirstOrDefault();
            if (someStop != null)
            {
                Name = someStop.Name.ToUpper();
            }
            else
            {
                Name = Utils.GetString("NoStopsPageTitle");
            }
            foreach (var stops in connectedStops)
            {
                var stopsVM = new StopPivotItemVM(stops);
                StopPivotItems.Add(stopsVM);
            }
            foreach (var stopsVM in StopPivotItems)
            {
                stopsVM.UpdateDepartures();
            }
        }

        protected override void Initialize()
        {
            foreach (var stopsVM in StopPivotItems)
            {
                stopsVM.UpdateDepartures();
            }
        }
    }
}
