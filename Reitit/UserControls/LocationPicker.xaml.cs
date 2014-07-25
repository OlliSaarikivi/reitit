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

    public class LocationPickerFavoriteOpacityConverter : LambdaConverter<IPickerLocation, double, object>
    {
        public LocationPickerFavoriteOpacityConverter()
            : base((s, p, language) =>
            {
                return ShouldBeVisible(s) ? 1 : 0;
            }) { }

        public static bool ShouldBeVisible(IPickerLocation location)
        {
            return !(location == null || location is FavoritePickerLocation || location is MeLocation);
        }
    }

    public sealed partial class LocationPicker : UserControl
    {
        private Color? _oldStatusBarColor;

        public bool IsInFavoriteMode
        {
            get { return (bool)GetValue(IsInFavoriteModeProperty); }
            set { SetValue(IsInFavoriteModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInFavoriteMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInFavoriteModeProperty =
            DependencyProperty.Register("IsInFavoriteMode", typeof(bool), typeof(LocationPicker), new PropertyMetadata(false));

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
        }

        private void FlyoutButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var point = e.GetPosition(FlyoutButton);
            if (point.X > FlyoutButton.ActualWidth - 45 && LocationPickerFavoriteOpacityConverter.ShouldBeVisible(Value) && !IsInFavoriteMode)
            {
                AddFavoriteFlyout.ShowAt(FlyoutButton);
            }
            else
            {
                App.Current.LocationPickerFlyout.Show(x => Value = x, IsInFavoriteMode);
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
            var statusBar = StatusBar.GetForCurrentView();
            _oldStatusBarColor = statusBar.ForegroundColor;
            statusBar.ForegroundColor = (Color)App.Current.Resources["PhoneForegroundColor"];

            AddFavoriteName.Text = Value.Name;
            AddFavoriteName.SelectAll();
            AddFavoriteAccept.IsEnabled = (AddFavoriteName.Text.Length != 0);
        }

        private void AddFavoriteFlyout_Closed(object sender, object e)
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ForegroundColor = _oldStatusBarColor;
        }
    }
}
