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
                _vm.ContentMinimizedOffset = page.Height - page.MinimizedHeight;
                Binding binding = new Binding
                {
                    Path = new PropertyPath("ContentMaximized"),
                    Source = DataContext,
                    Mode = BindingMode.OneWay,
                };
                page.SetBinding(MapContentPage.IsMaximizedProperty, binding);
                page.NewPushpins += ContentPage_NewPushpins;
                _vm.Map.Pushpins = page.Pushpins;
            }
            else
            {
                throw new Exception("Invalid navigation: MapPage content pages must extend MapContentPage");
            }
        }

        private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var page = ContentFrame.Content as MapContentPage;
            if (page != null)
            {
                page.NewPushpins -= ContentPage_NewPushpins;
            }
        }

        void ContentPage_NewPushpins(ObservableCollection<PushpinVM> newPushpins)
        {
            _vm.Map.Pushpins = newPushpins;
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
            statusBar.ForegroundColor = Color.FromArgb(255, 0, 0, 0);

            Messenger.Default.Register<MapPageNavigateMessage>(this, HandleMapPageNavigateMessage);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ForegroundColor = (Color)App.Current.Resources["PhoneForegroundColor"];
        }

        private void HandleMapPageNavigateMessage(MapPageNavigateMessage message)
        {
            ContentFrame.Navigate(message.ContentPage, message.Parameter);
        }
        
        private async void MinimizerRectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Minimize();
        }

        private async Task Minimize()
        {
            VisualStateManager.GoToState(this, "ContentMinimized", true);
            _vm.ContentMaximized = false;
            await UpdateMapTransform();
        }

        private async void MaximizerRectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Maximize();
        }

        private async Task Maximize()
        {
            VisualStateManager.GoToState(this, "ContentMaximized", true);
            _vm.ContentMaximized = true;
            await UpdateMapTransform();
        }

        private async Task UpdateMapTransform()
        {
            var vm = (MapPageVM)DataContext;
            await Map.UpdateMapTransform(ContentFrame.ActualHeight - (vm.ContentMaximized ? 0 : vm.ContentMinimizedOffset));
        }
    }
}
