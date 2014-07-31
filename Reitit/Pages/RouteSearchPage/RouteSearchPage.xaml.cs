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
    public class AddAdvancedCommand { }

    public class AdvancedTombstoner : Tombstoner
    {
        public RouteSearchPage _page;

        public AdvancedTombstoner(RouteSearchPage page) : base("RouteSearchPageAdvanced")
        {
            _page = page;
        }
        public override void ResetState()
        {
            if (_page.AdvancedRoot.Child != null)
            {
                _page.AdvancedRoot.Child = null;
            }
            if (_page.Advanced != null)
            {
                _page.Advanced.DataContext = null;
                _page.Advanced.Visibility = Visibility.Collapsed;
                VisualStateManager.GoToState(_page.Advanced, "Hidden", false);
            }
            _page.AdvancedButton.Visibility = Visibility.Visible;
        }

        protected override void SaveState(IDictionary<string, object> state)
        {
            state["Added"] = _page.AdvancedRoot.Child != null;
        }

        protected override void RestoreState(IDictionary<string, object> state)
        {
            if ((bool)state["Added"])
            {
                _page.AddAdvanced(false);
            }
            else
            {
                ResetState();
            }
        }
    }

    public sealed partial class RouteSearchPage : MapContentPage
    {
        public ExtraSearchParameters Advanced { get; set; }
        private MapLocation _currentMenuLocation;

        public RouteSearchPage()
        {
            this.InitializeComponent();
            Tombstoners.Add(new ScrollViewerTombstoner(ContentScroll));
            Tombstoners.Add(new AdvancedTombstoner(this));
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

        private void AdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            AddAdvanced(true);
        }

        public void AddAdvanced(bool useTransitions)
        {
            if (Advanced == null)
            {
                Advanced = new ExtraSearchParameters();
            }
            Advanced.DataContext = DataContext;
            AdvancedButton.Visibility = Visibility.Collapsed;
            if (AdvancedRoot.Child != Advanced)
            {
                AdvancedRoot.Child = Advanced;
            }
            Advanced.Visibility = Visibility.Visible;
            VisualStateManager.GoToState(Advanced, "Visible", useTransitions);
        }
    }
}
