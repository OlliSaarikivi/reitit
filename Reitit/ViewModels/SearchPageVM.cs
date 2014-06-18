using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Oat;
using Reitit.Resources;
using ReittiAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Reitit
{
    public enum SearchResultType
    {
        Stops, Location, Line
    }

    [DataContract]
    public class SearchResultVM : ViewModelBase
    {
        public Brush IconBackground { get; set; }
        public ImageSource Icon { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Detail { get; set; }
        [DataMember]
        public SearchResultType Type { get; set; }
        [DataMember]
        public List<ConnectedStops> Stops { get; set; }
        [DataMember]
        public Location Loc { get; set; }
        [DataMember]
        public ConnectedLines Line { get; set; }

        public SearchResultVM(string name, List<ConnectedStops> stops)
        {
            Type = SearchResultType.Stops;
            Name = name;
            Detail = string.Join(", ", from stop in stops
                                       select stop.DisplayCode);
            Stops = stops;
        }

        public SearchResultVM(Location loc)
        {
            Type = SearchResultType.Location;
            Name = loc.MatchedLongName;
            Loc = loc;
        }

        public SearchResultVM(ConnectedLines line)
            : base(false)
        {
            Type = SearchResultType.Line;
            var example = line.Lines[0];
            Name = Utils.FormatLineCode(example.ShortName, example.TransportTypeId);
            Detail = Utils.GetPreferredName(example.NamesByLang);
            Line = line;
            Initialize();
        }

        protected override void Initialize()
        {
            if (Type == SearchResultType.Line)
            {
                string typeId = Line.Lines[0].TransportTypeId.ToString();
                IconBackground = Utils.GetStrokeForType(typeId);
                Icon = Utils.GetIconForType(typeId);
            }
        }
    }

    [DataContract]
    public class SearchResultGroup : ObservableCollection<SearchResultVM>
    {
        [DataMember]
        public string Key { get; set; }
    }

    [DataContract]
    class SearchPageVM : ViewModelBase
    {
        protected override void Initialize()
        {

        }

        public string SearchTerm
        {
            get { return _searchTerm; }
            set { Set(() => SearchTerm, ref _searchTerm, value); }
        }
        [DataMember]
        public string _searchTerm = "";

        public SearchResultVM SelectedResult
        {
            get { return null; }
            set
            {
                ResultClicked(value);
                RaisePropertyChanged(() => SelectedResult);
            }
        }

        private void ResultClicked(SearchResultVM value)
        {
            MessageBox.Show("Click!");
        }

        public ObservableCollection<SearchResultGroup> ResultGroups
        {
            get { return _resultGroups; }
        }
        [DataMember]
        public ObservableCollection<SearchResultGroup> _resultGroups = new ObservableCollection<SearchResultGroup>();

        public bool JumpingEnabled
        {
            get { return _jumpingEnabled; }
            set { Set(() => JumpingEnabled, ref _jumpingEnabled, value); }
        }
        [DataMember]
        public bool _jumpingEnabled = true;

        public bool HasSearched
        {
            get { return _hasSearched; }
            set { Set(() => HasSearched, ref _hasSearched, value); }
        }
        [DataMember]
        public bool _hasSearched = false;

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
            bool hasWritten = false;
            PropertyChangedEventHandler textChangedHandler = (s, e) =>
            {
                if (e.PropertyName == "SearchTerm")
                {
                    hasWritten = true;
                }
            };
            PropertyChanged += textChangedHandler;
            if (SearchTerm.Length >= 3)
            {
                try
                {
                    using (new TrayStatus(AppResources.SearchingStatus))
                    {
                        if (Utils.GetIsNetworkAvailableAndWarn())
                        {
                            try
                            {
                                var trimmed = SearchTerm.Trim();
                                var lineTask = App.Current.ReittiClient.LineInformationAsync(trimmed.Split(' '), cancellationToken: token);
                                var geocodeResults = await App.Current.ReittiClient.GeocodeAsync(trimmed, cancellationToken: token);
                                var lineResults = await lineTask;
                                token.ThrowIfCancellationRequested();

                                SelectedResult = null;
                                ResultGroups.Clear();

                                var lineResultsGroup = new SearchResultGroup { Key = AppResources.ListLinesHeader };
                                lineResultsGroup.AddRange(from line in lineResults
                                                          select new SearchResultVM(line));
                                if (lineResultsGroup.Count > 0)
                                {
                                    ResultGroups.Add(lineResultsGroup);
                                }

                                if (geocodeResults.ConnectedStopsList.Count > 0)
                                {
                                    var connectedStopsByName = new Dictionary<string, List<ConnectedStops>>();
                                    foreach (var connectedStops in geocodeResults.ConnectedStopsList)
                                    {
                                        List<ConnectedStops> stopsList;
                                        if (!connectedStopsByName.TryGetValue(connectedStops.Locations[0].MatchedLongName, out stopsList))
                                        {
                                            stopsList = new List<ConnectedStops>();
                                            connectedStopsByName[connectedStops.Locations[0].MatchedLongName] = stopsList;
                                        }
                                        stopsList.Add(connectedStops);
                                    }
                                    var stopResults = new SearchResultGroup { Key = AppResources.LocationPickerStopsHeader };
                                    stopResults.AddRange(from entry in connectedStopsByName
                                                         select new SearchResultVM(entry.Key, entry.Value));
                                    ResultGroups.Add(stopResults);
                                }
                                if (geocodeResults.Pois.Count > 0)
                                {
                                    var placeResults = new SearchResultGroup { Key = AppResources.LocationPickerPlacesHeader };
                                    placeResults.AddRange(from loc in geocodeResults.Pois
                                                          select new SearchResultVM(loc));
                                    ResultGroups.Add(placeResults);
                                }
                                if (geocodeResults.Addresses.Count + geocodeResults.Streets.Count + geocodeResults.OtherAddresses.Count > 0)
                                {
                                    var addressResults = new SearchResultGroup { Key = AppResources.LocationPickerAddressesHeader };
                                    addressResults.AddRange(from loc in geocodeResults.Addresses
                                                            select new SearchResultVM(loc));
                                    addressResults.AddRange(from loc in geocodeResults.Streets
                                                            select new SearchResultVM(loc));
                                    addressResults.AddRange(from loc in geocodeResults.OtherAddresses
                                                            select new SearchResultVM(loc));
                                    ResultGroups.Add(addressResults);
                                }
                                if (geocodeResults.Others.Count > 0)
                                {
                                    var otherResults = new SearchResultGroup { Key = AppResources.LocationPickerOtherHeader };
                                    otherResults.AddRange(from loc in geocodeResults.Others
                                                          select new SearchResultVM(loc));
                                    ResultGroups.Add(otherResults);
                                }

                                JumpingEnabled = ResultGroups.Count > 1 && (from grp in ResultGroups
                                                                                    select grp.Count).Sum() > 6;

                                HasSearched = true;

                                // Scroll to top
                                Messenger.Default.Send(new NewResults
                                {
                                    ScrollTo = ResultGroups.Count != 0 ? ResultGroups[0] : null,
                                    HasWritten = hasWritten
                                });
                            }
                            catch (ReittiAPIException)
                            {
                                token.ThrowIfCancellationRequested();
                                MessageBox.Show(AppResources.SearchFailed, AppResources.SearchFailedTitle, MessageBoxButton.OK);
                            }
                        }
                    }
                }
                finally
                {
                    PropertyChanged -= textChangedHandler;
                }
            }
        }
    }
}
