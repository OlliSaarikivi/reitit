using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Reitit
{
    public sealed partial class EditFavPage : MapContentPage
    {
        private MapLocation _currentMenuLocation;

        public EditFavPage()
        {
            this.InitializeComponent();
            Tombstoners.Add(new ScrollViewerTombstoner(ContentScroll));
        }

        protected override object ConstructVM(object parameter)
        {
            return new EditFavPageVM(parameter as FavoriteLocation);
        }

        public override void OnIsMaximizedChanged()
        {
            AppBar.Visibility = IsMaximized ? Visibility.Visible : Visibility.Collapsed;
        }

        public override void OnMapHolding(FrameworkElement source, ReittiCoordinate coordinate)
        {
            _currentMenuLocation = new MapLocation
            {
                Coordinate = coordinate,
            };
            _currentMenuLocation.UpdateNameFromReverseGeocode();
            MapFlyout.ShowAt(source);
        }

        private void MapFlyoutFavItem_Click(object sender, RoutedEventArgs e)
        {
            ((EditFavPageVM)DataContext).Coordinate = _currentMenuLocation;
        }
    }
}
