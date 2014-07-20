using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.ViewManagement;
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
    public class MapPageNavigateMessage
    {
        public MapPageNavigateMessage(Type contentPage, object parameter)
        {
            ContentPage = contentPage;
            Parameter = parameter;
        }
        public Type ContentPage { get; set; }
        public object Parameter { get; set; }
    }

    public class ShowMyLocationImplicitMessage
    {
        public bool Show { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : PageBase
    {
        private MapPageVM _vm;

        public MapPage()
        {
            this.InitializeComponent();
            ContentFrame.SizeChanged += ContentFrame_SizeChanged;
        }

        protected override async void OnBackPressed(BackPressedEventArgs e)
        {
            if (DataContext != null && !_vm.ContentMaximized)
            {
                await Maximize();
                e.Handled = true;
            }
        }

        async void ContentFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            await UpdateMapTransform();
        }

        void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var page = ContentFrame.Content as MapContentPage;
            if (page != null)
            {
                ((HostingNavigationHelper)NavigationHelper).HostedHelper = page.NavigationHelper;
                ContentFrame.Height = page.Height;
                Map.BottomObscuredHeight = page.Height;
                _vm.ContentMinimizedHeight = page.MinimizedHeight;
                _vm.ContentMinimizedOffset = page.Height - page.MinimizedHeight;
                Binding binding = new Binding
                {
                    Path = new PropertyPath("ContentMaximized"),
                    Source = DataContext,
                    Mode = BindingMode.OneWay,
                };
                page.SetBinding(MapContentPage.IsMaximizedProperty, binding);
                Map.RegisterItems(page.MapItems);
                Messenger.Default.Register<ShowMyLocationImplicitMessage>(this, page, ShowMyLocationImplicitChanged);
            }
            else
            {
                throw new Exception("Invalid navigation: MapPage content pages must extend MapContentPage");
            }
        }

        private async void ShowMyLocationImplicitChanged(ShowMyLocationImplicitMessage message)
        {
            if (message.Show)
            {
                await _vm.ShowMyLocationImplicit();
            }
            else
            {
                _vm.ClearShowMyLocationImplicit();
            }
        }

        private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var page = ContentFrame.Content as MapContentPage;
            if (page != null)
            {
                Map.UnregisterItems(page.MapItems);
            }
        }

        protected override NavigationHelper ConstructNavigation()
        {
            return new HostingNavigationHelper(this);
        }

        protected override object ConstructVM(object parameter)
        {
            _vm = new MapPageVM();
            // A bit ugly, but it works
            DataContext = _vm;
            var message = (MapPageNavigateMessage)parameter;
            ContentFrame.BackStack.Clear();
            HandleMapPageNavigateMessage(message);
            return DataContext;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.BackgroundOpacity = 0.5;
            statusBar.BackgroundColor = ((SolidColorBrush)App.Current.Resources["PhoneBackgroundBrush"]).Color;

            Messenger.Default.Register<MapPageNavigateMessage>(this, HandleMapPageNavigateMessage);

            if (_vm.ContentMaximized)
            {
                Maximize(false);
            }
            else
            {
                Minimize(false);
            }

            Map.RegisterItems(MapItems);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ForegroundColor = (Color)App.Current.Resources["PhoneForegroundColor"];

            var page = ContentFrame.Content as MapContentPage;
            if (page != null)
            {
                Map.UnregisterItems(page.MapItems);
            }

            Map.UnregisterItems(MapItems);
        }

        private void HandleMapPageNavigateMessage(MapPageNavigateMessage message)
        {
            ContentFrame.Navigate(message.ContentPage, message.Parameter);
        }

        private async Task Minimize(bool animate = true)
        {
            VisualStateManager.GoToState(this, "ContentMinimized", animate);
            _vm.ContentMaximized = false;
            await UpdateMapTransform();
        }

        private async void MaximizerRectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Maximize();
        }

        private async Task Maximize(bool animate = true)
        {
            VisualStateManager.GoToState(this, "ContentMaximized", animate);
            _vm.ContentMaximized = true;
            await UpdateMapTransform();
        }

        private async Task UpdateMapTransform()
        {
            var vm = (MapPageVM)DataContext;
            Map.UpdateMapTransform(ContentFrame.ActualHeight - (vm.ContentMaximized ? 0 : vm.ContentMinimizedOffset));
        }

        private async void MapTouched()
        {
            await Minimize();
        }
    }
}
