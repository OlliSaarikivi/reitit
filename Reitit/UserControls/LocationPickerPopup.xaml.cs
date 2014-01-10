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
using ReittiAPI;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Media;
using BindableApplicationBar;

namespace Reitit
{
    public partial class LocationPickerPopup : LocationPickerPopupBase
    {
        private bool _searchBoxLoaded = false;

        public LocationPickerPopup()
        {
            InitializeComponent();

            DoneButton.Command = new RelayCommand(async () =>
            {
                var selection = ((LocationPickerPopupVM)DataContext).VisibleSelection;
                if (((LocationPickerPopupVM)DataContext).SelectedPivotTag == ((LocationPickerPopupVM)DataContext).SearchTag)
                {
                    App.Current.Recent.Add(new RecentLocation
                    {
                        Name = selection.Name,
                        Coordinate = await selection.GetCoordinates(), // Should complete synchronously
                    });
                }
                Done(selection);
            });
            CancelButton.Command = new RelayCommand(() =>
            {
                Done(null);
            });
            MeButton.Command = new RelayCommand(() =>
            {
                Done(MeLocation.Instance);
            });
            AddFavMenuItem.Command = new RelayCommand(async () =>
            {
                var vm = (LocationPickerPopupVM)DataContext;
                var fav = new FavoriteLocation
                {
                    Name = vm.VisibleSelection.Name,
                    Coordinate = await vm.VisibleSelection.GetCoordinates(),
                    IconIndex = IconManager.DefaultFallbackIndex,
                    LocationName = vm.VisibleSelection.Name
                };
                App.Current.Favorites.Add(fav);
            });
        }
        protected override void InitializeWithCurrent(IPickerLocation current)
        {
            DataContext = new LocationPickerPopupVM(this);

            if (_searchBoxLoaded)
            {
                Dispatcher.BeginInvoke(async () =>
                {
                    await Task.Delay(30);
                    SearchBox.Focus();
                });
            }
            else
            {
                SearchBox.Loaded += SearchBox_Loaded;
            }
        }

        void SearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            _searchBoxLoaded = true;
            SearchBox.Focus();
            SearchBox.Loaded -= SearchBox_Loaded;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var binding = (sender as TextBox).GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }
    }

    class LocationPickerPopupVM : ExtendedObservableObject
    {
        private LocationPickerPopup _view;

        public DerivedProperty<IPickerLocation> VisibleSelectionProperty;
        public IPickerLocation VisibleSelection { get { return VisibleSelectionProperty.Get(); } }

        public DerivedProperty<bool> IsAddFavEnabledProperty;
        public bool IsAddFavEnabled { get { return IsAddFavEnabledProperty.Get(); } }

        public DerivedProperty<bool> NoResultsProperty;
        public bool NoResults { get { return NoResultsProperty.Get(); } }

        public LocationPickerPopupVM(LocationPickerPopup view)
        {
            _view = view;

            VisibleSelectionProperty = CreateDerivedProperty(() => VisibleSelection,
                () => SelectedPivotTag == SearchTag ? (IPickerLocation)SelectedResult :
                      SelectedPivotTag == FavoritesTag ? (IPickerLocation)SelectedFavorite :
                      SelectedPivotTag == RecentTag ? (IPickerLocation)SelectedRecentLocation : null);

            if (_view.Pivot.SelectedItem != null)
            {
                SelectedPivotTag = ((PivotItem)_view.Pivot.SelectedItem).Tag;
            }
            else
            {
                RoutedEventHandler pivotLoaded = null;
                pivotLoaded = (s, e) =>
                {
                    SelectedPivotTag = ((PivotItem)_view.Pivot.SelectedItem).Tag;
                    _view.Pivot.Loaded -= pivotLoaded;
                };
                _view.Pivot.Loaded += pivotLoaded;
            }

            IsAddFavEnabledProperty = CreateDerivedProperty(() => IsAddFavEnabled,
                () => SelectedPivotTag != FavoritesTag && VisibleSelection != null);

            NoResultsProperty = CreateDerivedProperty(() => NoResults,
                () => HasSearched && ResultLocationGroups.Count == 0);

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "VisibleSelection")
                {
                    _view.DoneButton.IsEnabled = VisibleSelection != null;
                }
                else if (e.PropertyName == "IsAddFavEnabled")
                {
                    _view.AddFavMenuItem.IsEnabled = IsAddFavEnabled;
                }
            };
        }

        public RelayCommand<SelectionChangedEventArgs> PivotChangedCommand
        {
            get
            {
                return new RelayCommand<SelectionChangedEventArgs>((e) =>
                {
                    var tag = ((PivotItem)e.AddedItems[0]).Tag;
                    SelectedPivotTag = tag;
                });
            }
        }
        
        public string SearchTerm
        {
            get { return _searchTerm; }
            set { Set(() => SearchTerm, ref _searchTerm, value); }
        }
        private string _searchTerm = "";

        public object SelectedPivotTag
        {
            get { return _selectedPivotTag; }
            set { Set(() => SelectedPivotTag, ref _selectedPivotTag, value); }
        }
        private object _selectedPivotTag;

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
        public object RecentTag
        {
            get { return _recentTag; }
        }
        private static object _recentTag = new object();

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

        private CancellationTokenSource _searchTokenSource;
        public RelayCommand<KeyEventArgs> SearchCommand
        {
            get
            {
                return new RelayCommand<KeyEventArgs>(async (e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        if (_searchTokenSource != null)
                        {
                            _searchTokenSource.Cancel();
                        }
                        _searchTokenSource = new CancellationTokenSource();
                        try
                        {
                            await Search(_searchTokenSource.Token);
                        }
                        catch (OperationCanceledException) { }
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
                        if (Utils.GetIsNetworkAvailableAndWarn())
                        {
                            try
                            {
                                var result = await App.Current.ReittiClient.GeocodeAsync(SearchTerm.Trim(), cancellationToken: token);
                                token.ThrowIfCancellationRequested();

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

                                JumpingEnabled = ResultLocationGroups.Count > 1 && (from grp in ResultLocationGroups
                                                                                    select grp.Count).Sum() > 7;

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
                                token.ThrowIfCancellationRequested();
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
    }

    public class LocationGroup : ObservableCollection<ReittiLocationBase>
    {
        public string Key { get; set; }
    }
}
