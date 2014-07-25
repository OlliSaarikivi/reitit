using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
using Reitit.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Reitit
{
    public class ValidityToStringConverter : LambdaConverter<Validity, string, object>
    {
        public static string FormatInRelationToNow(DateTime time, DateTime? previousTime = null)
        {
            var previousDate = previousTime != null ? previousTime.Value.Date : DateTime.Now.Date;
            if (previousDate == time.Date)
            {
                return Utils.GetTimeFormatter().Format(time);
            }
            else
            {
                return Utils.GetDateTimeFormatter().Format(time);
            }
        }

        public ValidityToStringConverter()
            : base((v, p, language) =>
            {
                return string.Format(Utils.GetString("ValidityFormat"), FormatInRelationToNow(v.From), FormatInRelationToNow(v.To));
            }) { }
    }

    public sealed partial class HubPage : PageBase
    {
        HubPageVM VM { get { return DataContext as HubPageVM; } }
        ListView _favoritesListView;
        TextBlock _noFavoritesHint;

        public HubPage()
        {
            this.InitializeComponent();
            var flyout = App.Current.LocationPickerFlyout; // Preload the flyout
        }

        protected override object ConstructVM(object parameter)
        {
            return new HubPageVM();
        }

        protected override void OnShown()
        {
            App.Current.DisruptionsLoader.Load();

            App.Current.Favorites.SortedLocations.CollectionChanged += FavoritesChanged;
        }

        protected override void OnHiding()
        {
            App.Current.Favorites.SortedLocations.CollectionChanged -= FavoritesChanged;
        }

        private void FavoritesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _noFavoritesHint.Visibility = App.Current.Favorites.SortedLocations.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void Favorite_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Started)
            {
                var source = sender as FrameworkElement;
                if (source != null)
                {
                    VM.HeldFavorite = source.DataContext as FavoriteLocation;
                    if (VM.HeldFavorite != null)
                    {
                        FavoriteMenu.ShowAt(source);
                    }
                    else
                    {
                        await Utils.ShowOperationFailedError();
                    }
                }
                else
                {
                    await Utils.ShowOperationFailedError();
                }
            }
        }

        private async void EditFavoriteItem_Click(object sender, RoutedEventArgs e)
        {
            if (VM.HeldFavorite != null)
            {
                Utils.Navigate(typeof(EditFavPage), VM.HeldFavorite);
            }
            else
            {
                await Utils.ShowOperationFailedError();
            }
        }

        private async void DeleteFavoriteItem_Click(object sender, RoutedEventArgs e)
        {
            if (VM.HeldFavorite != null)
            {
                int id;
                if (App.Current.Favorites.Contains(VM.HeldFavorite, out id))
                {
                    App.Current.Favorites.Remove(id);
                }
                else
                {
                    await Utils.ShowOperationFailedError();
                }
            }
            else
            {
                await Utils.ShowOperationFailedError();
            }
        }

        private async void ReorderFavoriteItem_Click(object sender, RoutedEventArgs e)
        {
            if (_favoritesListView != null)
            {
                _favoritesListView.ReorderMode = ListViewReorderMode.Enabled;
            }
            else
            {
                await Utils.ShowOperationFailedError();
            }
        }

        private void FavoritesListView_Loaded(object sender, RoutedEventArgs e)
        {
            _favoritesListView = sender as ListView;
        }

        private void Hub_SectionsInViewChanged(object sender, SectionsInViewChangedEventArgs e)
        {
            if (!Hub.SectionsInView.Contains(FavoritesSection))
            {
                _favoritesListView.ReorderMode = ListViewReorderMode.Disabled;
            }
        }

        protected override void OnBackPressed(BackPressedEventArgs e)
        {
            if (_favoritesListView.ReorderMode == ListViewReorderMode.Enabled)
            {
                e.Handled = true;
                _favoritesListView.ReorderMode = ListViewReorderMode.Disabled;
            }
        }

        private void FavoritesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_favoritesListView.ReorderMode == ListViewReorderMode.Enabled)
            {
                _favoritesListView.ReorderMode = ListViewReorderMode.Disabled;
            }
            else
            {
                var fav = e.ClickedItem as FavoriteLocation;
                if (fav != null)
                {
                    Utils.Navigate(typeof(RouteSearchPage), new FavoritePickerLocation(fav));
                }
                else
                {
                    Utils.ShowOperationFailedError();
                }
            }
        }

        private void Hub_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _favoritesListView.ReorderMode = ListViewReorderMode.Disabled;
        }

        private void AddFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.Navigate(typeof(EditFavPage), null);
        }

        private void TrafficInfoScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            Tombstoners.Add(new ScrollViewerTombstoner(sender as ScrollViewer));
        }

        private void NoFavoritesHint_Loaded(object sender, RoutedEventArgs e)
        {
            _noFavoritesHint = sender as TextBlock;
            _noFavoritesHint.Visibility = App.Current.Favorites.SortedLocations.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
