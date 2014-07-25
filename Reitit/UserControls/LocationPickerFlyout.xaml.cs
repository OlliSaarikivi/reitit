using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.PersonalInformation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Reitit
{
    public sealed partial class LocationPickerFlyout : UserControl
    {
        private AppBar _oldBar;
        private Color? _oldStatusBarColor;
        private CommandBar _commandBar;
        private AppBarButton _acceptButton;
        private LocationPickerFlyoutVM _flyoutVM = new LocationPickerFlyoutVM();
        private bool _closingTemporarily;
        private string _searchWith;
        private Action<IPickerLocation> _setValue;
        private bool _isInFavoriteMode;

        public LocationPickerFlyout()
        {
            this.InitializeComponent();

            _commandBar = new CommandBar();
            _acceptButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = Utils.GetString("PickerDone"),
                Command = new RelayCommand(async () =>
                {
                    var result = _flyoutVM.SelectedResult;
                    if (_setValue != null)
                    {
                        _setValue(result);
                    }
                    Flyout.Hide();
                    // Only add remote results to recent
                    if (_flyoutVM.SelectedResult is ReittiLocationBase)
                    {
                        App.Current.Recent.Add(new RecentLocation
                        {
                            Name = result.Name,
                            Detail = result.Detail,
                            Coordinate = await result.GetCoordinates()
                        });
                    }
                }),
                IsEnabled = _flyoutVM.SelectedResult != null,
            };
            _commandBar.PrimaryCommands.Add(_acceptButton);
            _commandBar.PrimaryCommands.Add(new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Cancel),
                Label = Utils.GetString("PickerCancel"),
                Command = new RelayCommand(() =>
                {
                    Flyout.Hide();
                }),
            });
            var meButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Target),
                Label = Utils.GetString("PickerMe"),
                Command = new RelayCommand(() =>
                {
                    if (_setValue != null)
                    {
                        _setValue(MeLocation.Instance);
                    }
                    Flyout.Hide();
                }),
            };
            _commandBar.PrimaryCommands.Add(meButton);
            Binding binding = new Binding
            {
                Path = new PropertyPath("IsInFavoriteMode"),
                Source = this,
                Mode = BindingMode.OneWay,
                Converter = new NegVisibilityConverter(),
            };
            meButton.SetBinding(AppBarButton.VisibilityProperty, binding);

            FlyoutRoot.DataContext = _flyoutVM;
        }

        public void Show(Action<IPickerLocation> setValue, bool useFavoriteMode)
        {
            _setValue = setValue;
            _isInFavoriteMode = useFavoriteMode;
            Flyout.ShowAt(Window.Current.Content as Frame);
        }

        private void Flyout_Opening(object sender, object e)
        {
            _flyoutVM.Init(_isInFavoriteMode);

            var frame = (Frame)Window.Current.Content;
            var page = (Page)frame.Content;
            _oldBar = page.BottomAppBar;
            page.BottomAppBar = _commandBar;

            if (!_closingTemporarily)
            {
                var statusBar = StatusBar.GetForCurrentView();
                _oldStatusBarColor = statusBar.ForegroundColor;
                statusBar.ForegroundColor = (Color)App.Current.Resources["PhoneForegroundColor"];
            }

            Messenger.Default.Register<ScrollIntoViewMessage>(this, _flyoutVM, m =>
            {
                LocationsListView.ScrollIntoView(m.Item, ScrollIntoViewAlignment.Default);
            });
            Messenger.Default.Register<UnfocusTextBoxMessage>(this, _flyoutVM, async m =>
            {
                await Utils.OnCoreDispatcher(() =>
                {
                    UnfocusButton.Focus(FocusState.Programmatic);
                });
            });
            Messenger.Default.Register<DontStopListeningMessage>(this, _flyoutVM, m =>
            {
                _closingTemporarily = true;
            });
            Messenger.Default.Register<SearchContactMessage>(this, _flyoutVM, m =>
            {
                if (m.Contact == null)
                {
                    _searchWith = null;
                    Flyout.ShowAt(Window.Current.Content as Frame);
                }
                else if (m.Contact.Addresses.Count > 1)
                {
                    var picker = (ListPickerFlyout)Resources["AddressPickerFlyout"];
                    picker.ItemsSource = m.Contact.Addresses;
                    picker.ItemTemplate = (DataTemplate)Resources["AddressPickerItemTemplate"];
                    EventHandler<object> closedHandler = null;
                    TypedEventHandler<ListPickerFlyout, ItemsPickedEventArgs> pickedHandler = null;
                    closedHandler = (s, e3) =>
                    {
                        picker.Closed -= closedHandler;
                        picker.ItemsPicked -= pickedHandler;
                        Messenger.Default.Unregister(this);
                    };
                    pickedHandler = (s, e2) =>
                    {
                        picker.Closed -= closedHandler;
                        picker.ItemsPicked -= pickedHandler;
                        var address = (ContactAddress)e2.AddedItems[0];
                        _searchWith = address.StreetAddress + (address.Locality != "" ? ", " + address.Locality : "");
                        Flyout.ShowAt(Window.Current.Content as Frame);
                    };
                    picker.Closed += closedHandler;
                    picker.ItemsPicked += pickedHandler;
                    picker.SelectionMode = ListPickerFlyoutSelectionMode.Single;
                    picker.ShowAt(this);
                }
                else if (m.Contact.Addresses.Count == 1)
                {
                    var address = m.Contact.Addresses[0];
                    _searchWith = address.StreetAddress + (address.Locality != "" ? ", " + address.Locality : "");
                    Flyout.ShowAt(Window.Current.Content as Frame);
                }
            });

            if (_searchWith != null)
            {
                _flyoutVM.SearchTerm = _searchWith;
                _flyoutVM.Search();
            }

            _closingTemporarily = false;
        }

        private void Flyout_Opened(object sender, object e)
        {
            if (_searchWith == null)
            {
                SearchBox.Focus(FocusState.Programmatic);
            }
            else
            {
                _searchWith = null;
            }
        }

        private void Flyout_Closed(object sender, object e)
        {
            var frame = (Frame)Window.Current.Content;
            var page = (Page)frame.Content;
            page.BottomAppBar = _oldBar;
            _oldBar = null;

            if (!_closingTemporarily)
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = _oldStatusBarColor;
                Messenger.Default.Unregister(this);
            }

            _flyoutVM.Clear();
        }

        private void LocationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _acceptButton.IsEnabled = _flyoutVM.SelectedResult != null;
        }

        private void GroupHeaderBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //SemanticZoom.IsZoomedInViewActive = false;
        }

        private void SemanticZoom_ViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.IsSourceZoomedInView)
                return;

            var selectedGroup = e.DestinationItem.Item as LocationGroup;
            if (selectedGroup == null)
                return;

            LocationsListView.ScrollIntoView(selectedGroup, ScrollIntoViewAlignment.Leading);
        }

        public void Clear()
        {
            _flyoutVM.Clear();
        }
    }
}
