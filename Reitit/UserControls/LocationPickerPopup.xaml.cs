using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Reitit.Resources;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;
using Oat;

namespace Reitit
{
    public partial class LocationPickerPopup : LocationPickerPopupBase
    {
        public LocationPickerPopup()
        {
            InitializeComponent();

            DoneButton.Command = new RelayCommand(() =>
            {
                throw new NotImplementedException();
            });
            CancelButton.Command = new RelayCommand(() =>
            {
                Done(null);
            });
        }
        protected override void InitializeWithCurrent(IPickerLocation current)
        {
            DataContext = new LocationPickerPopupVM();
        }
    }

    class LocationPickerPopupVM : ObservableObject
    {
        public string SearchTerm
        {
            get { return _searchTerm; }
            set { Set(() => SearchTerm, ref _searchTerm, value); }
        }
        private string _searchTerm;

        public bool NoResultsVisible
        {
            get { return _noResultsVisible; }
            set { Set(() => NoResultsVisible, ref _noResultsVisible, value); }
        }
        private bool _noResultsVisible = false;

        public object SearchTag
        {
            get { return _searchTag; }
        }
        private static object _searchTag = new object();
        public object MapTag
        {
            get { return _mapTag; }
        }
        private static object _mapTag = new object();
        public object FavoritesTag
        {
            get { return _favoritesTag; }
        }
        private static object _favoritesTag = new object();

        public List<LocationGroup> ResultLocationGroups
        {
            get { return _resultLocationGroups; }
        }
        private List<LocationGroup> _resultLocationGroups = new List<LocationGroup>
        {
            new LocationGroup { Key = AppResources.LocationPickerPlacesHeader },
            new LocationGroup { Key = AppResources.LocationPickerAddressesHeader },
            new LocationGroup { Key = AppResources.LocationPickerStopsHeader },
            new LocationGroup { Key = AppResources.LocationPickerOtherHeader },
        };
        public LocationGroup PlaceResults { get { return ResultLocationGroups[0]; } }
        public LocationGroup AddressResults { get { return ResultLocationGroups[1]; } }
        public LocationGroup StopResults { get { return ResultLocationGroups[2]; } }
        public LocationGroup OtherResults { get { return ResultLocationGroups[3]; } }
        public bool GroupResults
        {
            get
            {
                return (from grp in ResultLocationGroups
                        select grp.Count).Sum() > 7;

            }
        }

        private CancellationTokenSource _searchTokenSource = new CancellationTokenSource();
        public RelayCommand<KeyEventArgs> SearchCommand
        {
            get
            {
                return new RelayCommand<KeyEventArgs>(async (e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        _searchTokenSource.Cancel();
                        _searchTask = Search();
                    }
                });
            }
        }

        private async Task Search(CancellationToken token)
        {
            if (SearchTerm.Length < 3)
            {
                MessageBox.Show(AppResources.LocationPickerShortSearchPrompt, AppResources.LocationPickerShortSearchPromptTitle, MessageBoxButton.OK);
            }
            else
            {
                using (new TrayStatus(AppResources.LocationSearching))
                {
                    if (Utils.GetIsNetworkAvailableAndWarn())
                    {
                        var result = await App.Current.ReittiClient.GeocodeAsync(SearchTerm, cancellationToken: token);

                        result.

                                foreach (var location in result.Addresses)
                                {
                                    var routingCoordinate = new LocationCoordinate
                                    {
                                        Location = location
                                    };
                                    SearchResultsAddresses.Items.Add(routingCoordinate);
                                }

                                foreach (var location in result.Pois)
                                {
                                    var routingCoordinate = new LocationCoordinate
                                    {
                                        Location = location
                                    };
                                    SearchResultsPois.Items.Add(routingCoordinate);
                                }

                                foreach (var location in result.Streets)
                                {
                                    var routingCoordinate = new LocationCoordinate
                                    {
                                        Location = location
                                    };
                                    SearchResultsAddresses.Items.Add(routingCoordinate);
                                }

                                foreach (var connectedStops in result.ConnectedStopsList)
                                {
                                    var routingCoordinate = new StopCoordinate
                                    {
                                        Stop = connectedStops.Locations[0],
                                    };
                                    //var routingCoordinate = new CustomCoordinate
                                    //{
                                    //    ShortName = connectedStops.LongName,
                                    //    LongName = connectedStops.LongName + " (" + connectedStops.Locations[0].ShortCode + ")",
                                    //    Coordinate = connectedStops.Locations[0].Coords,
                                    //    Type = RoutingCoordinateType.Other
                                    //};
                                    SearchResultsStops.Items.Add(routingCoordinate);
                                }

                                foreach (var location in result.OtherAddresses)
                                {
                                    var routingCoordinate = new LocationCoordinate
                                    {
                                        Location = location
                                    };
                                    SearchResultsAddresses.Items.Add(routingCoordinate);
                                }

                                foreach (var location in result.Others)
                                {
                                    var routingCoordinate = new LocationCoordinate
                                    {
                                        Location = location
                                    };
                                    SearchResultsOthers.Items.Add(routingCoordinate);
                                }

                                UpdateSearchMisc();
                            }
                        }
                        catch (ReittiAPIException e)
                        {
                            bool ignore;
                            if (_downloadTokens.TryGetValue(myToken, out ignore))
                            {
                                _loading = false;
                                _indicator.IsVisible = false;
                                MessageBox.Show(String.Format(Res.PickLocsFailedFormat, e.Message), Res.MBError, MessageBoxButton.OK);

                                UpdateSearchMisc();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(Res.GenericNoConnectivity, Res.GenericNoConnectivityTitle, MessageBoxButton.OK);
                    }
                }

                await SearchLocation(myToken, SearchBox.Text.Trim());
                Focus();
            }
        }
    }

    public class LocationGroup : List<ReittiLocation>
    {
        public string Key { get; set; }
    }
}
