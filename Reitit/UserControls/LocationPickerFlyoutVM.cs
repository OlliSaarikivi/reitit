using GalaSoft.MvvmLight.Command;
using Reitit.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;

namespace Reitit
{
    class LocationPickerFlyoutVM : ViewModelBase
    {
        public DerivedProperty<bool> NoResultsProperty;
        public bool NoResults { get { return NoResultsProperty.Get(); } }

        public LocationPickerFlyoutVM()
        {
            NoResultsProperty = CreateDerivedProperty(() => NoResults,
                () => HasSearched && ResultLocationGroups.Count == 0);
        }
        
        public string SearchTerm
        {
            get { return _searchTerm; }
            set { Set(() => SearchTerm, ref _searchTerm, value); }
        }
        private string _searchTerm = "";

        public ObservableCollection<LocationGroup> ResultLocationGroups
        {
            get { return _resultLocationGroups; }
        }
        private ObservableCollection<LocationGroup> _resultLocationGroups = new ObservableCollection<LocationGroup>();

        public bool JumpingEnabled
        {
            get { return _jumpingEnabled; }
            set { Set(() => JumpingEnabled, ref _jumpingEnabled, value); }
        }
        private bool _jumpingEnabled = true;

        public ReittiLocationBase SelectedResult
        {
            get { return _selectedResult; }
            set
            {
                if (_selectedResult != null)
                {
                    _selectedResult.Selected = false;
                }
                Set(() => SelectedResult, ref _selectedResult, value);
                if (_selectedResult != null)
                {
                    _selectedResult.Selected = true;
                }
            }
        }
        private ReittiLocationBase _selectedResult;



        public TransformObservableCollection<FavoritePickerLocation, FavoriteLocation> Favorites
        {
            get
            {
                return _favorites;
            }
        }
        private TransformObservableCollection<FavoritePickerLocation, FavoriteLocation> _favorites
            = new TransformObservableCollection<FavoritePickerLocation, FavoriteLocation>(App.Current.Favorites.SortedLocations, fav => new FavoritePickerLocation(fav));

        public FavoritePickerLocation SelectedFavorite
        {
            get { return _selectedFavorite; }
            set
            {
                if (_selectedFavorite != null)
                {
                    _selectedFavorite.Selected = false;
                }
                Set(() => SelectedFavorite, ref _selectedFavorite, value);
                if (_selectedFavorite != null)
                {
                    _selectedFavorite.Selected = true;
                }
            }
        }
        private FavoritePickerLocation _selectedFavorite;




        public TransformObservableCollection<RecentPickerLocation, RecentLocation> RecentLocations
        {
            get
            {
                return _recentLocations;
            }
        }
        private TransformObservableCollection<RecentPickerLocation, RecentLocation> _recentLocations
            = new TransformObservableCollection<RecentPickerLocation, RecentLocation>(App.Current.Recent.Locations, recent => new RecentPickerLocation(recent));

        public RecentPickerLocation SelectedRecentLocation
        {
            get { return _selectedRecentLocation; }
            set
            {
                if (_selectedRecentLocation != null)
                {
                    _selectedRecentLocation.Selected = false;
                }
                Set(() => SelectedRecentLocation, ref _selectedRecentLocation, value);
                if (_selectedRecentLocation != null)
                {
                    _selectedRecentLocation.Selected = true;
                }
            }
        }
        private RecentPickerLocation _selectedRecentLocation;

        public bool HasSearched
        {
            get { return _hasSearched; }
            set { Set(() => HasSearched, ref _hasSearched, value); }
        }
        private bool _hasSearched = false;

        public RelayCommand<KeyEventArgs> SearchCommand
        {
            get
            {
                return new RelayCommand<KeyEventArgs>(async (e) =>
                {
                    if (e.VirtualKey == VirtualKey.Enter)
                    {
                        await Search();
                    }
                });
            }
        }

        private CancellationTokenSource _searchTokenSource;
        public async Task Search()
        {
            if (_searchTokenSource != null)
            {
                _searchTokenSource.Cancel();
            }
            _searchTokenSource = new CancellationTokenSource();
            try
            {
                if (SearchTerm.Length < 3)
                {
                    throw new NotImplementedException();
                    //MessageBox.Show(AppResources.LocationPickerShortSearchPrompt, AppResources.LocationPickerShortSearchPromptTitle, MessageBoxButton.OK);
                }
                else
                {
                    bool hasWritten = false;
                    TextChangedEventHandler textChangedHandler = (s, e) =>
                    {
                        hasWritten = true;
                    };
                    _view.SearchBox.TextChanged += textChangedHandler;
                    try
                    {
                        using (new TrayStatus(AppResources.SearchingStatus))
                        {
                            if (await Utils.GetIsNetworkAvailableAndWarn())
                            {
                                try
                                {
                                    var result = await App.Current.ReittiClient.GeocodeAsync(SearchTerm.Trim(), cancellationToken: _searchTokenSource.Token);
                                    _searchTokenSource.Token.ThrowIfCancellationRequested();

                                    SelectedResult = null;
                                    ResultLocationGroups.Clear();
                                    if (result.Pois.Count > 0)
                                    {
                                        var placeResults = new LocationGroup { Key = AppResources.LocationPickerPlacesHeader };
                                        placeResults.AddRange(from loc in result.Pois
                                                              select new ReittiLocation(loc));
                                        ResultLocationGroups.Add(placeResults);
                                    }
                                    if (result.Addresses.Count + result.Streets.Count + result.OtherAddresses.Count > 0)
                                    {
                                        var addressResults = new LocationGroup { Key = AppResources.LocationPickerAddressesHeader };
                                        addressResults.AddRange(from loc in result.Addresses
                                                                select new ReittiLocation(loc));
                                        addressResults.AddRange(from loc in result.Streets
                                                                select new ReittiLocation(loc));
                                        addressResults.AddRange(from loc in result.OtherAddresses
                                                                select new ReittiLocation(loc));
                                        ResultLocationGroups.Add(addressResults);
                                    }
                                    if (result.ConnectedStopsList.Count > 0)
                                    {
                                        var stopResults = new LocationGroup { Key = AppResources.LocationPickerStopsHeader };
                                        stopResults.AddRange(from stops in result.ConnectedStopsList
                                                             select new ReittiStopsLocation(stops));
                                        ResultLocationGroups.Add(stopResults);
                                    }
                                    if (result.Others.Count > 0)
                                    {
                                        var otherResults = new LocationGroup { Key = AppResources.LocationPickerOtherHeader };
                                        otherResults.AddRange(from loc in result.Others
                                                              select new ReittiLocation(loc));
                                        ResultLocationGroups.Add(otherResults);
                                    }

                                    var resultsCount = (from grp in ResultLocationGroups
                                                        select grp.Count).Sum();
                                    JumpingEnabled = ResultLocationGroups.Count > 1 && resultsCount > 7;

                                    if (resultsCount == 1)
                                    {
                                        SelectedResult = ResultLocationGroups[0][0];
                                    }

                                    HasSearched = true;

                                    // Scroll to top
                                    _view.UpdateLayout();
                                    if (ResultLocationGroups.Count != 0)
                                    {
                                        _view.SearchResultsSelector.ScrollTo(ResultLocationGroups[0]);
                                    }
                                    if (!hasWritten)
                                    {
                                        _view.Focus();
                                    }
                                }
                                catch (ReittiAPIException)
                                {
                                    _searchTokenSource.Token.ThrowIfCancellationRequested();
                                    MessageBox.Show(AppResources.LocationPickerSearchFailed, AppResources.LocationPickerSearchFailedTitle, MessageBoxButton.OK);
                                }
                            }
                        }
                    }
                    finally
                    {
                        _view.SearchBox.TextChanged -= textChangedHandler;
                    }
                }
            }
            catch (OperationCanceledException) { }
        }
    }

    public class LocationGroup : ObservableCollection<IPickerLocation>
    {
        public string Key { get; set; }
    }
}
