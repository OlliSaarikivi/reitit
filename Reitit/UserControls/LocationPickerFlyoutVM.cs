using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace Reitit
{
    public class LocationPickerFlyoutVM : ViewModelBase
    {
        public DerivedProperty<bool> NoResultsProperty;
        public bool NoResults { get { return NoResultsProperty.Get(); } }

        public DerivedProperty<bool> CanRevealLocalProperty;
        public bool CanRevealLocal { get { return CanRevealLocalProperty.Get(); } }

        public DerivedProperty<bool> ShowResultsProperty;
        public bool ShowResults { get { return ShowResultsProperty.Get(); } }

        public LocationPickerFlyoutVM()
        {
            NoResultsProperty = CreateDerivedProperty(() => NoResults,
                () => HasSearched && !SomeRemoteResults && (ResultLocationGroups.Count == 0 || !ShowEvenOnlyLocalResults));
            CanRevealLocalProperty = CreateDerivedProperty(() => CanRevealLocal,
                () => NoResults && ResultLocationGroups.Count != 0);
            ShowResultsProperty = CreateDerivedProperty(() => ShowResults,
                () => !NoResults && !CanRevealLocal);

            _resultLocationGroupsSource = new CollectionViewSource
            {
                Source = ResultLocationGroups,
                IsSourceGrouped = true,
            };
            Clear();
        }

        public void Clear()
        {
            SearchTerm = "";
            SearchMessage = "";
            ResultLocationGroups.Clear();
            AddFavorites();
            AddRecent();
            JumpingEnabled = true;
            if (ResultLocationGroups.Count > 0 && ResultLocationGroups[0].Count > 0)
            {
                SelectedResult = ResultLocationGroups[0][0];
            }
            else
            {
                SelectedResult = null;
            }
            HasSearched = false;
            SomeRemoteResults = false;
            ShowEvenOnlyLocalResults = false;
        }

        public string SearchTerm
        {
            get { return _searchTerm; }
            set { Set(() => SearchTerm, ref _searchTerm, value); }
        }
        private string _searchTerm;

        public string SearchMessage
        {
            get { return _searchMessage; }
            set { Set(() => SearchMessage, ref _searchMessage, value); }
        }
        private string _searchMessage;

        public ObservableCollection<LocationGroup> ResultLocationGroups
        {
            get { return _resultLocationGroups; }
        }
        private ObservableCollection<LocationGroup> _resultLocationGroups = new ObservableCollection<LocationGroup>();

        public CollectionViewSource ResultLocationGroupsSource
        {
            get { return _resultLocationGroupsSource; }
        }
        private CollectionViewSource _resultLocationGroupsSource;

        public ObservableCollection<string> Suggestions
        {
            get { return _suggestions; }
        }
        private ObservableCollection<string> _suggestions = new ObservableCollection<string>();

        public bool JumpingEnabled
        {
            get { return _jumpingEnabled; }
            set { Set(() => JumpingEnabled, ref _jumpingEnabled, value); }
        }
        private bool _jumpingEnabled;

        public bool SomeRemoteResults
        {
            get { return _someRemoteResults; }
            set { Set(() => SomeRemoteResults, ref _someRemoteResults, value); }
        }
        public bool _someRemoteResults;

        public bool ShowEvenOnlyLocalResults
        {
            get { return _showEvenOnlyLocalResults; }
            set { Set(() => ShowEvenOnlyLocalResults, ref _showEvenOnlyLocalResults, value); }
        }
        public bool _showEvenOnlyLocalResults;

        public SelectableLocation SelectedResult
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
                Messenger.Default.Send(new SelectionChangedMessage(), this);
            }
        }
        private SelectableLocation _selectedResult;

        //public TransformObservableCollection<FavoritePickerLocation, FavoriteLocation> Favorites { get { return _favorites; } }
        //private TransformObservableCollection<FavoritePickerLocation, FavoriteLocation> _favorites
        //    = new TransformObservableCollection<FavoritePickerLocation, FavoriteLocation>(App.Current.Favorites.SortedLocations, fav => new FavoritePickerLocation(fav));

        //public TransformObservableCollection<RecentPickerLocation, RecentLocation> RecentLocations { get { return _recentLocations; } }
        //private TransformObservableCollection<RecentPickerLocation, RecentLocation> _recentLocations
        //    = new TransformObservableCollection<RecentPickerLocation, RecentLocation>(App.Current.Recent.Locations, recent => new RecentPickerLocation(recent));

        public bool HasSearched
        {
            get { return _hasSearched; }
            set { Set(() => HasSearched, ref _hasSearched, value); }
        }
        private bool _hasSearched;

        public RelayCommand<KeyRoutedEventArgs> KeyDownCommand
        {
            get
            {
                return new RelayCommand<KeyRoutedEventArgs>(async (e) =>
                {
                    if (e.Key == VirtualKey.Enter)
                    {
                        await Search();
                    }
                });
            }
        }

        public RelayCommand RevealLocalCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ShowEvenOnlyLocalResults = true;
                    Messenger.Default.Send(new UnfocusTextBoxMessage(), this);
                });
            }
        }

        public RelayCommand SelectContactCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    var picker = new ContactPicker();
                    picker.DesiredFieldsWithContactFieldType.Add(ContactFieldType.Address);
                    Messenger.Default.Send(new DontStopListeningMessage(), this);
                    var contact = await picker.PickContactAsync();
                    Messenger.Default.Send(new SearchContactMessage { Contact = contact }, this);
                });
            }
        }

        public RelayCommand<AutoSuggestBoxTextChangedEventArgs> TextChangedCommand
        {
            get
            {
                return new RelayCommand<AutoSuggestBoxTextChangedEventArgs>((e) =>
                {
                    if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                    {
                        _hasWritten = true;
                        Suggestions.Clear();
                        if (SearchTerm.Length > 0)
                        {
                            Suggestions.AddRange(App.Current.SearchHistory.SearchesWithPrefix(SearchTerm));
                        }
                    }
                });
            }
        }
        private bool _hasWritten;

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
                string sanitizedSearch = SearchTerm.Trim();
                if (sanitizedSearch.Length < 3)
                {
                    SearchMessage = Utils.GetString("LocationPickerShortSearchMessage");
                }
                else
                {
                    SearchMessage = null;
                    _hasWritten = false;
                    await Utils.UsingStatus(Utils.GetString("SearchingStatus"), async () =>
                    {
                        if (Utils.GetIsNetworkAvailableAndWarn())
                        {
                            try
                            {
                                var result = await App.Current.ReittiClient.GeocodeAsync(sanitizedSearch, cancellationToken: _searchTokenSource.Token);
                                _searchTokenSource.Token.ThrowIfCancellationRequested();

                                SelectedResult = null;
                                ResultLocationGroups.Clear();
                                if (result.Pois.Count > 0)
                                {
                                    var placeResults = new LocationGroup { Key = Utils.GetString("LocationPickerPlacesHeader") };
                                    placeResults.AddRange(from loc in result.Pois
                                                            select new ReittiLocation(loc));
                                    ResultLocationGroups.Add(placeResults);
                                }
                                if (result.Addresses.Count + result.Streets.Count + result.OtherAddresses.Count > 0)
                                {
                                    var addressResults = new LocationGroup { Key = Utils.GetString("LocationPickerAddressesHeader") };
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
                                    var stopResults = new LocationGroup { Key = Utils.GetString("LocationPickerStopsHeader") };
                                    stopResults.AddRange(from stops in result.ConnectedStopsList
                                                            select new ReittiStopsLocation(stops));
                                    ResultLocationGroups.Add(stopResults);
                                }
                                if (result.Others.Count > 0)
                                {
                                    var otherResults = new LocationGroup { Key = Utils.GetString("LocationPickerOtherHeader") };
                                    otherResults.AddRange(from loc in result.Others
                                                            select new ReittiLocation(loc));
                                    ResultLocationGroups.Add(otherResults);
                                }

                                var resultsCount = (from grp in ResultLocationGroups
                                                    select grp.Count).Sum();

                                SomeRemoteResults = (resultsCount != 0);
                                ShowEvenOnlyLocalResults = false;

                                AddFavorites(sanitizedSearch);
                                AddRecent(sanitizedSearch);

                                resultsCount = (from grp in ResultLocationGroups
                                                select grp.Count).Sum();
                                JumpingEnabled = ResultLocationGroups.Count > 1 && resultsCount > 7;


                                if (ResultLocationGroups.Count > 0 && ResultLocationGroups[0].Count > 0)
                                {
                                    SelectedResult = ResultLocationGroups[0][0];
                                }

                                HasSearched = true;

                                // Scroll to top
                                if (ResultLocationGroups.Count != 0)
                                {
                                    Messenger.Default.Send(new ScrollIntoViewMessage { Item = ResultLocationGroups[0] }, this);
                                }
                                if (!_hasWritten)
                                {
                                    Messenger.Default.Send(new UnfocusTextBoxMessage(), this);
                                }
                            }
                            catch (ReittiAPIException)
                            {
                                _searchTokenSource.Token.ThrowIfCancellationRequested();
                                Utils.ShowMessageDialog(Utils.GetString("LocationPickerSearchFailed"), Utils.GetString("LocationPickerSearchFailedTitle"));
                            }
                        }
                    });
                }
            }
            catch (OperationCanceledException) { }
        }

        private void AddFavorites(string prefix = "")
        {
            var favoritesGroup = new LocationGroup { Key = Utils.GetString("LocationPickerFavoritesHeader") };
            favoritesGroup.AddRange<SelectableLocation>(from favorite in App.Current.Favorites.LocationsWithPrefix(prefix)
                                                        select new FavoritePickerLocation(favorite));
            if (favoritesGroup.Count > 0)
            {
                ResultLocationGroups.Add(favoritesGroup);
            }
        }

        private void AddRecent(string prefix = "")
        {
            var recentGroup = new LocationGroup { Key = Utils.GetString("LocationPickerRecentHeader") };
            recentGroup.AddRange<SelectableLocation>(from recent in App.Current.Recent.LocationsWithPrefix(prefix)
                                                     select new RecentPickerLocation(recent));
            if (recentGroup.Count > 0)
            {
                ResultLocationGroups.Add(recentGroup);
            }
        }
    }

    public class LocationGroup : ObservableCollection<SelectableLocation>
    {
        public string Key { get; set; }
    }
}
