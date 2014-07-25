using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
    public class SearchTermChangedMessage
    {
        public string Text { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StopSearchPage : MapContentPage
    {
        private ReittiCoordinate _currentMenuCoordinate;

        public StopSearchPage()
        {
            this.InitializeComponent();
        }

        protected override object ConstructVM(object parameter)
        {
            return new StopSearchPageVM();
        }

        public override void OnMapHolding(FrameworkElement source, ReittiCoordinate coordinate)
        {
            _currentMenuCoordinate = coordinate;
            MapFlyout.ShowAt(source);
        }

        private async void MapFlyoutNearHereItem_Click(object sender, RoutedEventArgs e)
        {
            await ((StopSearchPageVM)DataContext).SearchInArea(_currentMenuCoordinate);
        }

        protected override void OnShown()
        {
            Messenger.Default.Register<SearchTermChangedMessage>(this, DataContext, m =>
            {
                SearchBox.Text = m.Text;
            });
            Messenger.Default.Register<ScrollIntoViewMessage>(this, DataContext, m =>
            {
                StopsListView.ScrollIntoView(m.Item, ScrollIntoViewAlignment.Default);
            });
            Messenger.Default.Register<UnfocusTextBoxMessage>(this, DataContext, async m =>
            {
                await Utils.OnCoreDispatcher(() =>
                {
                    UnfocusButton.Focus(FocusState.Programmatic);
                });
            });
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "SearchBoxFocused", true);
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "SearchBoxNotFocused", true);
        }

        private async void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                await ((StopSearchPageVM)DataContext).Search();
            }
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ((StopSearchPageVM)DataContext).SearchBoxTextChanged(SearchBox.Text);
        }
    }
}
