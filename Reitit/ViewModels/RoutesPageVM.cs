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

namespace Reitit
{
    public class RouteVM : ObservableObject
    {
        public CompoundRoute Route { get; private set; }

        public bool Selected
        {
            get { return _selected; }
            set { Set(() => Selected, ref _selected, value); }
        }
        private bool _selected = false;

        public RouteVM(CompoundRoute route)
        {
            Route = route;
        }
    }

    [DataContract]
    public class RoutesPageVM : ViewModelBase
    {
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
