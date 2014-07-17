using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public class ScrollIntoViewMessage
    {
        public object Item { get; set; }
    }
    public class UnfocusTextBoxMessage { }
    public class SelectionChangedMessage { }
    public class DontStopListeningMessage { }
    public class SearchContactMessage
    {
        public Contact Contact { get; set; }
    }

    public class LocationPickerFavoriteVisibilityConverter : LambdaConverter<IPickerLocation, Visibility, object>
    {
        public LocationPickerFavoriteVisibilityConverter()
            : base((s, p, language) =>
            {
                return ShouldBeVisible(s) ? Visibility.Visible : Visibility.Collapsed;
            }) { }

        public static bool ShouldBeVisible(IPickerLocation location)
        {
            return !(location == null || location is FavoritePickerLocation || location is MeLocation);
        }
    }

    public sealed partial class LocationPicker : UserControl
    {
        private AppBar _oldBar;
        private Color? _oldStatusBarColor;
        private CommandBar _commandBar;
        private AppBarButton _acceptButton;
        private LocationPickerFlyoutVM _flyoutVM = new LocationPickerFlyoutVM();
        private bool _closingTemporarily;
        private string _searchWith;

        public IPickerLocation Value
        {
            get { return (IPickerLocation)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Location.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(LocationPicker), new PropertyMetadata(null));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(LocationPicker), new PropertyMetadata(null));

        public LocationPicker()
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
                    Value = result;
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
            _commandBar.PrimaryCommands.Add(new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Target),
                Label = Utils.GetString("PickerMe"),
                Command = new RelayCommand(() =>
                {
                    Value = MeLocation.Instance;
                    Flyout.Hide();
                }),
            });

            FlyoutRoot.DataContext = _flyoutVM;
        }

        private void Flyout_Opening(object sender, object e)
        {
            _flyoutVM.Clear();

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
            Messenger.Default.Register<UnfocusTextBoxMessage>(this, _flyoutVM, m =>
            {
                this.Focus(FocusState.Programmatic);
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
                    Flyout.ShowAt(FlyoutButton);
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
                        Flyout.ShowAt(FlyoutButton);
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
                    Flyout.ShowAt(FlyoutButton);
                }
            });

            if (_searchWith != null)
            {
                _flyoutVM.SearchTerm = _searchWith;
                _flyoutVM.Search();
                _searchWith = null;
            }

            _closingTemporarily = false;
        }

        private void Flyout_Opened(object sender, object e)
        {
            if (_flyoutVM.SearchTerm == "")
            {
                SearchBox.Focus(FocusState.Programmatic);
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
        }

        private void LocationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _acceptButton.IsEnabled = _flyoutVM.SelectedResult != null;
        }

        private void FlyoutButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var point = e.GetPosition(FlyoutButton);
            if (point.X > FlyoutButton.ActualWidth - 45 && LocationPickerFavoriteVisibilityConverter.ShouldBeVisible(Value))
            {
                AddFavoriteFlyout.ShowAt(FlyoutButton);
            }
            else
            {
                Flyout.ShowAt(FlyoutButton);
            }
            e.Handled = true;
        }

        private void AddFavoriteCancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddFavoriteFlyout.Hide();
        }

        private async void AddFavoriteAccept_Click(object sender, RoutedEventArgs e)
        {
            AddFavoriteFlyout.Hide();
            var name = AddFavoriteName.Text;
            var value = Value;
            var favorite = new FavoriteLocation
            {
                Name = name,
                LocationName = value.Name,
                Coordinate = await value.GetCoordinates(),
            };
            App.Current.Favorites.Add(favorite);
            Value = new FavoritePickerLocation(favorite);
        }

        private void AddFavoriteNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddFavoriteAccept.IsEnabled = (AddFavoriteName.Text.Length != 0);
        }

        private void AddFavoriteFlyout_Opening(object sender, object e)
        {
            AddFavoriteName.Text = Value.Name;
            AddFavoriteAccept.IsEnabled = (AddFavoriteName.Text.Length != 0);
        }
    }
}
