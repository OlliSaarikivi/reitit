using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
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
using Windows.UI.Xaml.Media.Animation;
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

    public class MapPageGoBackMessage { }

    public class ShowMyLocationImplicitMessage
    {
        public bool Show { get; set; }
    }

    public class MapSetViewMessage
    {
        public ReittiCoordinate Center { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : PageBase
    {
        private bool _frameRegistered = false;

        private MapPageVM VM
        {
            get
            {
                return DataContext as MapPageVM;
            }
        }

        public MapPage()
        {
            this.InitializeComponent();
            ContentFrame.SizeChanged += ContentFrame_SizeChanged;
            ContentFrame.ContentTransitions = new TransitionCollection() {
                new NavigationThemeTransition
                {
                    DefaultNavigationTransitionInfo = new SlideNavigationTransitionInfo(),
                }
            };
        }

        protected override void OnShown()
        {
            Messenger.Default.Register<MapPageNavigateMessage>(this, HandleMapPageNavigateMessage);
            Messenger.Default.Register<MapSetViewMessage>(this, HandleSetViewMessgage);
            Messenger.Default.Register<MapPageGoBackMessage>(this, HandleGoBackMessage);

            if (VM.ContentMaximized)
            {
                Maximize(false);
            }
            else
            {
                Minimize(false);
            }

            Map.RegisterItems(MapItems);
            Map.RegisterMapElements(MapElements);
        }

        protected override void OnHiding()
        {
            VM.ClearShowMyLocation();

            var page = ContentFrame.Content as MapContentPage;
            if (page != null)
            {
                Map.UnregisterItems(page.MapItems);
                Map.UnregisterMapElements(page.MapElements);
            }

            Map.UnregisterItems(MapItems);
            Map.UnregisterMapElements(MapElements);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bool setupBindings = false;
            if (!_frameRegistered)
            {
                _frameRegistered = true;
                SuspensionManager.RegisterFrame(ContentFrame, "MapContentFrame-" + Frame.BackStackDepth);
                if (e.NavigationMode == NavigationMode.Back)
                {
#if !DEBUG
                    try
                    {
#endif
                    SuspensionManager.RestoreFrame(ContentFrame);
                    setupBindings = true;
#if !DEBUG
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue.
                    }
#endif
                }
            }
            base.OnNavigatedTo(e);
            if (setupBindings)
            {
                SetupBindings();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back && _frameRegistered)
            {
                _frameRegistered = false;
                ContentFrame.BackStack.Clear();
                ContentFrame.BackStack.Add(new PageStackEntry(typeof(DummyPage), null, new SlideNavigationTransitionInfo()));
                Utils.OnCoreDispatcher(() =>
                {
                    ContentFrame.GoBack();
                    SuspensionManager.UnregisterFrame(ContentFrame);
                });
            }
        }

        protected override void OnBackPressed(BackPressedEventArgs e)
        {
            if (DataContext != null && !VM.ContentMaximized)
            {
                Maximize();
                e.Handled = true;
            }
        }

        void ContentFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMapTransform();
        }

        void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SetupBindings();
        }

        void SetupBindings()
        {
            var page = ContentFrame.Content as MapContentPage;
            if (page != null)
            {
                ((HostingNavigationHelper)NavigationHelper).HostedHelper = page.NavigationHelper;
                ContentFrame.Height = page.Height;
                VM.ContentMinimizedHeight = page.MinimizedHeight;
                VM.ContentMinimizedOffset = page.Height - page.MinimizedHeight;
                Binding binding = new Binding
                {
                    Path = new PropertyPath("ContentMaximized"),
                    Source = DataContext,
                    Mode = BindingMode.OneWay,
                };
                page.SetBinding(MapContentPage.IsMaximizedProperty, binding);
                Map.RegisterItems(page.MapItems);
                Map.RegisterMapElements(page.MapElements);
                Messenger.Default.Register<ShowMyLocationImplicitMessage>(this, page, ShowMyLocationImplicitChanged);
                Messenger.Default.Send(new ShowMyLocationImplicitMessage { Show = page.ShowMyLocationImplicit }, page);
            }
            else if (ContentFrame.Content is DummyPage)
            {
                // It's alright
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
                await VM.SetShowMyLocationImplicit();
            }
            else
            {
                VM.ClearShowMyLocationImplicit();
            }
        }

        private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var page = ContentFrame.Content as MapContentPage;
            if (page != null)
            {
                Map.UnregisterItems(page.MapItems);
                Map.UnregisterMapElements(page.MapElements);
                Messenger.Default.Unregister<ShowMyLocationImplicitMessage>(this, page);
            }
        }

        protected override NavigationHelper ConstructNavigation()
        {
            return new HostingNavigationHelper(this);
        }

        protected override object ConstructVM(object parameter)
        {
            // A bit ugly, but it works
            DataContext = new MapPageVM();
            var message = (MapPageNavigateMessage)parameter;
            HandleMapPageNavigateMessage(message);
            ContentFrame.BackStack.Clear();
            return DataContext;
        }

        private void HandleMapPageNavigateMessage(MapPageNavigateMessage message)
        {
            ContentFrame.Navigate(message.ContentPage, message.Parameter);
        }

        private async void HandleSetViewMessgage(MapSetViewMessage message)
        {
            await Map.SetView(message.Center, Utils.ShowLocationZoom);
        }

        private void HandleGoBackMessage(MapPageGoBackMessage obj)
        {
            NavigationHelper.GoBack();
        }

        private void Minimize(bool animate = true)
        {
            VisualStateManager.GoToState(this, "ContentMinimized", animate);
            VM.ContentMaximized = false;
            UpdateMapTransform();
            Map.DoAutofocusView();
        }

        private void MaximizerRectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Maximize();
        }

        private void Maximize(bool animate = true)
        {
            VisualStateManager.GoToState(this, "ContentMaximized", animate);
            VM.ContentMaximized = true;
            UpdateMapTransform();
        }

        private void UpdateMapTransform()
        {
            Map.UpdateMapTransform(ContentFrame.ActualHeight - (VM.ContentMaximized ? 0 : VM.ContentMinimizedOffset));
        }

        private void Minimizer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            VM.StopTracking();
            Minimize();
        }

        private void Map_ReititHolding(ReittiCoordinate coordinate, Point position)
        {
            var page = ContentFrame.Content as MapContentPage;
            if (page != null)
            {
                Canvas.SetLeft(MenuPositioner, position.X);
                Canvas.SetTop(MenuPositioner, position.Y);
                page.OnMapHolding(MenuPositioner, coordinate);
            }
        }
    }
}
