﻿using Reitit.API;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Reitit
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RouteSearchPage : MapContentPage
    {
        private MapLocation _currentMenuLocation;

        public RouteSearchPage()
        {
            this.InitializeComponent();
            Tombstoners.Add(new ScrollViewerTombstoner(ContentScroll));
        }

        protected override object ConstructVM(object parameter)
        {
            var to = parameter as IPickerLocation;
            return new RouteSearchPageVM { To = to };
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

        public override void OnIsMaximizedChanged()
        {
            AppBar.Visibility = IsMaximized ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MapFlyoutFromItem_Click(object sender, RoutedEventArgs e)
        {
            ((RouteSearchPageVM)DataContext).From = _currentMenuLocation;
        }

        private void MapFlyoutToItem_Click(object sender, RoutedEventArgs e)
        {
            ((RouteSearchPageVM)DataContext).To = _currentMenuLocation;
        }
    }
}
