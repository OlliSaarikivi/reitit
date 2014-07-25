using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Reitit
{
    [DataContract]
    class MultiStopResultVM
    {
        [DataMember]
        public List<ConnectedStops> Stops = new List<ConnectedStops>();

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        [DataMember]
        public string _name;

        public string Detail
        {
            get { return _detail; }
            set { _detail = value; }
        }
        [DataMember]
        public string _detail;

        public ReittiCoordinate Center
        {
            get { return _center; }
            set { _center = value; }
        }
        [DataMember]
        public ReittiCoordinate _center;
    }

    [DataContract]
    class StopResultVM
    {
        [DataMember]
        public ConnectedStops Stop;

        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }
        [DataMember]
        public string _label;

        public ReittiCoordinate Center
        {
            get { return _center; }
            set { _center = value; }
        }
        [DataMember]
        public ReittiCoordinate _center;
    }

    [DataContract]
    class StopSearchPageVM : ViewModelBase
    {
        public DerivedProperty<bool> NoResultsProperty;
        public bool NoResults { get { return NoResultsProperty.Get(); } }

        protected override void Initialize()
        {
            NoResultsProperty = CreateDerivedProperty(() => NoResults,
                () => HasSearched && Results.Count == 0);
        }

        public bool HasSearched
        {
            get { return _hasSearched; }
            set { Set(() => HasSearched, ref _hasSearched, value); }
        }
        [DataMember]
        public bool _hasSearched;

        public string SearchMessage
        {
            get { return _searchMessage; }
            set { Set(() => SearchMessage, ref _searchMessage, value); }
        }
        [DataMember]
        public string _searchMessage;

        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                bool changed = _searchTerm != value;
                Set(() => SearchTerm, ref _searchTerm, value);
                if (changed)
                {
                    Messenger.Default.Send(new SearchTermChangedMessage { Text = _searchTerm }, this);
                }
            }
        }
        [DataMember]
        public string _searchTerm = "";

        public ObservableCollection<string> Suggestions { get { return _suggestions; } }
        [DataMember]
        private ObservableCollection<string> _suggestions = new ObservableCollection<string>();

        public ObservableCollection<MultiStopResultVM> Results { get { return _results; } }
        [DataMember]
        public ObservableCollection<MultiStopResultVM> _results = new ObservableCollection<MultiStopResultVM>();

        public ObservableCollection<StopResultVM> ResultStops { get { return _resultStops; } }
        [DataMember]
        public ObservableCollection<StopResultVM> _resultStops = new ObservableCollection<StopResultVM>();

        public void SearchBoxTextChanged(string text)
        {
            SearchTerm = text;
            Suggestions.Clear();
            if (SearchTerm.Length > 0)
            {
                Suggestions.AddRange(App.Current.StopSearchHistory.SearchesWithPrefix(SearchTerm));
            }
        }

        public RelayCommand<AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosenCommand
        {
            get
            {
                return new RelayCommand<AutoSuggestBoxSuggestionChosenEventArgs>(async e =>
                {
                    SearchTerm = e.SelectedItem as string ?? "";
                    await Search();
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
                string sanitizedSearch = SearchTerm.Trim();
                if (sanitizedSearch.Length < 3)
                {
                    SearchMessage = Utils.GetString("ShortSearchMessage");
                }
                else
                {
                    Messenger.Default.Send(new UnfocusTextBoxMessage(), this);
                    SearchMessage = null;
                    await Utils.UsingStatus(Utils.GetString("StopsSearchingStatus"), async () =>
                    {
                        if (Utils.GetIsNetworkAvailableAndWarn())
                        {
                            try
                            {
                                var result = await App.Current.ReittiClient.GeocodeAsync(sanitizedSearch, locTypes: new string[] { "stop" }, cancellationToken: _searchTokenSource.Token);
                                _searchTokenSource.Token.ThrowIfCancellationRequested();
                                SetResults(result.ConnectedStopsList);
                                HasSearched = true;

                                // Scroll to top
                                if (Results.Count != 0)
                                {
                                    Messenger.Default.Send(new ScrollIntoViewMessage { Item = Results[0] }, this);
                                }

                                if (Results.Count > 0)
                                {
                                    App.Current.StopSearchHistory.Add(sanitizedSearch);
                                }
                            }
                            catch (ReittiAPIException)
                            {
                                _searchTokenSource.Token.ThrowIfCancellationRequested();
                                Utils.ShowMessageDialog(Utils.GetString("StopsSearchFailed"), Utils.GetString("StopsSearchFailedTitle"));
                            }
                        }
                    });
                }
            }
            catch (OperationCanceledException) { }
        }

        public async Task SearchInArea(ReittiCoordinate center)
        {
            if (_searchTokenSource != null)
            {
                _searchTokenSource.Cancel();
            }
            _searchTokenSource = new CancellationTokenSource();
            try
            {
                await Utils.UsingStatus(Utils.GetString("StopsSearchingStatus"), async () =>
                {
                    if (Utils.GetIsNetworkAvailableAndWarn())
                    {
                        try
                        {
                            var result = await App.Current.ReittiClient.StopsInAreaAsyc(center, limit: 50, diameter: 1000, cancellationToken: _searchTokenSource.Token);
                            _searchTokenSource.Token.ThrowIfCancellationRequested();

                            if (result.Count() == 0)
                            {
                                await Utils.UsingStatus(Utils.GetString("NoStopsInAreaMessage"), async () =>
                                {
                                    await Task.Delay(Utils.StatusBarNotificationTime);
                                });
                            }
                            else
                            {
                                SetResults(result.Select(x => new ConnectedStops(x)));
                                SearchMessage = null;
                                Messenger.Default.Send(new ScrollIntoViewMessage { Item = Results[0] }, this);
                            }
                        }
                        catch (ReittiAPIException)
                        {
                            _searchTokenSource.Token.ThrowIfCancellationRequested();
                            Utils.ShowMessageDialog(Utils.GetString("StopsSearchFailed"), Utils.GetString("StopsSearchFailedTitle"));
                        }
                    }
                });
            }
            catch (OperationCanceledException) { }
        }

        private void SetResults(IEnumerable<ConnectedStops> results)
        {
            var byName = from stop in results
                         group stop by stop.Name into g
                         select g;

            Results.Clear();
            ResultStops.Clear();

            foreach (var group in byName)
            {
                string detail = null;
                foreach (var stop in group)
                {
                    if (detail != null)
                    {
                        detail += ", ";
                    }
                    detail += stop.DisplayCode;
                    ResultStops.Add(new StopResultVM
                    {
                        Stop = stop,
                        Label = stop.DisplayCode,
                        Center = stop.Center,
                    });
                }
                var stopsVM = new MultiStopResultVM
                {
                    Name = group.Key,
                    Detail = detail,
                    Center = group.Select(x => x.Center).AverageCenter(),
                };
                stopsVM.Stops.AddRange(group);

                Results.Add(stopsVM);
            }
        }
    }
}
