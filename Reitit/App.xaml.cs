using Reitit.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Geolocation;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Hub Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Reitit
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        public new static App Current { get { return (App)Application.Current; } }
        public Settings Settings { get; private set; }
        public StackIndicatorManager IndicatorManager { get; private set; }
        public ModelCache ModelCache { get; private set; }
        public ReittiAPIClient ReittiClient { get; private set; }
        public FavoritesManager Favorites { get; private set; }
        public RecentManager Recent { get; private set; }
        public SearchHistoryManager SearchHistory { get; private set; }
        public ParamCache ParamCache { get; private set; }
        public EasyGeolocator Geolocator { get; private set; }

        private TransitionCollection transitions;
        private Task _initTask;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;

            _initTask = Settings.Load().ContinueWith(t =>
            {
                Settings = t.Result;
                IndicatorManager = new StackIndicatorManager();
                ModelCache = new ModelCache(AppConfiguration.ModelCacheSize);
                ReittiClient = new ReittiAPIClient(AppConfiguration.ReittiAPIUser, AppConfiguration.ReittiAPIPass);
                Favorites = new FavoritesManager();
                Recent = new RecentManager();
                SearchHistory = new SearchHistoryManager();
                ParamCache = new ParamCache();
                Geolocator = new EasyGeolocator { DesiredAccuracy = PositionAccuracy.High, MovementThreshold = AppConfiguration.GPSMovementThreshold };
            });
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            await _initTask;

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active.
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page.
                rootFrame = new Frame();

                // Associate the frame with a SuspensionManager key.
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                // TODO: Change this value to a cache size that is appropriate for your application.
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate.
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue.
                    }
                }

                // Place the frame in the current Window.
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter.
                if (!rootFrame.Navigate(typeof(HubPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            // Ensure the current window is active.
            Window.Current.Activate();

            OnAllActivations();
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            await Settings.Save(Settings);
            deferral.Complete();
        }

        private void OnAllActivations()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigated += (s, e) =>
            {
                if (e.SourcePageType == typeof(MapPage))
                {
                    var statusBar = StatusBar.GetForCurrentView();
                    statusBar.BackgroundOpacity = 0.6666666;
                    statusBar.BackgroundColor = (Color)App.Current.Resources["PhoneBackgroundColor"];
                }
                else
                {

                    var statusBar = StatusBar.GetForCurrentView();
                    statusBar.BackgroundOpacity = 0;
                }
            };
        }
    }
}
