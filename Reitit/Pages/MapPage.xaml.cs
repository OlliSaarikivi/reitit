using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        public MapPage()
        {
            this.InitializeComponent();
            ContentFrame.SizeChanged += ContentFrame_SizeChanged;
        }

        void ContentFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Map.UpdateMapTransform((sender as FrameworkElement).ActualHeight);
        }

        void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var page = ContentFrame.Content as MapContentPage;
            if (page != null)
            {
                ((HostingNavigationHelper)NavigationHelper).HostedHelper = page.NavigationHelper;
                Binding binding = new Binding
                {
                    Path = new PropertyPath("Height"),
                    Source = page,
                    Mode = BindingMode.OneWay,
                };
                ContentFrame.SetBinding(Frame.HeightProperty, binding);
            }
            else
            {
                throw new Exception("Invalid navigation: MapPage content pages must extend MapContentPage");
            }
        }

        protected override NavigationHelper ConstructNavigation()
        {
            return new HostingNavigationHelper(this);
        }

        protected override object ConstructVM(object parameter)
        {
            var message = (MapPageNavigateMessage)parameter;
            ContentFrame.BackStack.Clear();
            HandleMapPageNavigateMessage(message);
            return null;
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
    }
}
