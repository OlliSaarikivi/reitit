using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Reitit
{
    public class RoutePushpinVM : ObservableObject
    {
        public Brush Background
        {
            get { return _background; }
            set { Set(() => Background, ref _background, value); }
        }
        [DataMember]
        public Brush _background;
        public string Label
        {
            get { return _label; }
            set { Set(() => Label, ref _label, value); }
        }
        [DataMember]
        public string _label;
        public ReittiCoordinate Coordinate
        {
            get { return _coordinate; }
            set { Set(() => Coordinate, ref _coordinate, value); }
        }
        [DataMember]
        public ReittiCoordinate _coordinate;
    }

    public class FocusPushpinVM : ObservableObject
    {
        public bool InAutofocus
        {
            get { return _inAutofocus; }
            set { Set(() => InAutofocus, ref _inAutofocus, value); }
        }
        [DataMember]
        public bool _inAutofocus;
        public ReittiCoordinate Coordinate
        {
            get { return _coordinate; }
            set { Set(() => Coordinate, ref _coordinate, value); }
        }
        [DataMember]
        public ReittiCoordinate _coordinate;
    }

    public class RouteVM : ObservableObject
    {
        public CompoundRoute Route { get; private set; }

        public ObservableCollection<RoutePushpinVM> Pushpins { get; private set; }

        public ObservableCollection<FocusPushpinVM> FocusPushpins { get; private set; }

        Dictionary<Leg, List<FocusPushpinVM>> _focusGroups = new Dictionary<Leg, List<FocusPushpinVM>>();

        public Leg FocusedLeg
        {
            get { return _focusedLeg; }
            set
            {
                Set(() => FocusedLeg, ref _focusedLeg, value);

                if (_focusedLeg != null)
                {
                    foreach (var pushpin in FocusPushpins)
                    {
                        pushpin.InAutofocus = false;
                    }
                    List<FocusPushpinVM> focus;
                    if (_focusGroups.TryGetValue(_focusedLeg, out focus))
                    {
                        foreach (var pushpin in focus)
                        {
                            pushpin.InAutofocus = true;
                        }
                    }
                }
                else
                {
                    foreach (var pushpin in FocusPushpins)
                    {
                        pushpin.InAutofocus = true;
                    }
                }
            }
        }
        public Leg _focusedLeg;

        public bool Selected
        {
            get { return _selected; }
            set { Set(() => Selected, ref _selected, value); }
        }
        private bool _selected = false;

        public RouteVM(CompoundRoute route)
        {
            Route = route;
            Pushpins = new ObservableCollection<RoutePushpinVM>();
            FocusPushpins = new ObservableCollection<FocusPushpinVM>();
            foreach (var subRoute in route.Routes)
            {
                for (int i = 0; i < subRoute.Legs.Length; ++i)
                {
                    var leg = subRoute.Legs[i];
                    bool isFirst = i == 0;
                    bool isLast = i == subRoute.Legs.Length - 1;
                    if (isFirst || leg.Type != "walk")
                    {
                        var pushpinVM = new RoutePushpinVM
                        {
                            Background = (isFirst && leg.Type == "walk") ? (Brush)App.Current.Resources["StartBrush"] : Utils.BrushForType(leg.Type),
                            Label = leg.Type != "walk" ? leg.Line.ShortName : "",
                            Coordinate = leg.Locs[0].Coord,
                        };
                        Pushpins.Add(pushpinVM);
                    }
                    var focusGroup = new List<FocusPushpinVM>();
                    var bounds = Utils.GetBoundingBox(from loc in leg.Locs select loc.Coord);
                    focusGroup.Add(new FocusPushpinVM { Coordinate = new ReittiCoordinate(bounds.North, bounds.East), InAutofocus = true });
                    focusGroup.Add(new FocusPushpinVM { Coordinate = new ReittiCoordinate(bounds.South, bounds.West), InAutofocus = true });
                    _focusGroups.Add(leg, focusGroup);
                    FocusPushpins.AddRange(focusGroup);
                }
            }
        }
    }

    [DataContract]
    public class RoutesPageVM : ViewModelBase
    {
        static RoutesPageVM() { SuspensionManager.KnownTypes.Add(typeof(RoutesPageVM)); }

        public RouteLoader Loader { get { return _loader; } }
        [DataMember]
        public RouteLoader _loader;

        private DerivedProperty<bool> NoResultsProperty;
        public bool NoResults { get { return NoResultsProperty.Get(); } }

        public TransformObservableCollection<RouteVM, CompoundRoute> RouteVMs { get { return _routeVMs; } }
        private TransformObservableCollection<RouteVM, CompoundRoute> _routeVMs;

        public RouteVM SelectedRoute
        {
            get { return _selectedRoute; }
            set
            {
                if (value != null)
                {
                    Messenger.Default.Send(new ScrollToCurrentMessage());
                    value.Selected = true;
                }
                if (_selectedRoute != null)
                {
                    _selectedRoute.Selected = false;
                }
                Set(() => SelectedRoute, ref _selectedRoute, value);
            }
        }
        private RouteVM _selectedRoute;

        public RelayCommand PreviousCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await Loader.LoadPrevious();
                    Messenger.Default.Send(new KeepScrollMessage());
                });
            }
        }

        public RelayCommand NextCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    await Loader.LoadNext();
                });
            }
        }

        public RoutesPageVM(RouteLoader loader)
            : base(false)
        {
            _loader = loader;
            Initialize();
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            Initialize();
        }

        private void Initialize()
        {
            NoResultsProperty = CreateDerivedProperty(() => NoResults,
                () => Loader.LoadedRoutes.Count == 0 && Loader.InitialStatus == null);

            _routeVMs = new TransformObservableCollection<RouteVM, CompoundRoute>(Loader.LoadedRoutes, r => new RouteVM(r));
        }
    }
}
