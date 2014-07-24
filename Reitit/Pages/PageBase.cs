using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Reitit
{
    public abstract class PageBase : Page
    {
        public NavigationHelper NavigationHelper { get; private set; }

        public ObservableCollection<object> MapItems { get; private set; }

        public ObservableCollection<MapElement> MapElements { get; private set; }

        public object MapElementsSource
        {
            get { return (object)GetValue(MapElementsSourceProperty); }
            set { SetValue(MapElementsSourceProperty, value); }
        }
        public static readonly DependencyProperty MapElementsSourceProperty =
            DependencyProperty.Register("MapElementsSource", typeof(object), typeof(PageBase), new PropertyMetadata(null, MapElementsSourceChanged));
        private static void MapElementsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = d as PageBase;
            if (page != null)
            {
                page.MapElements.Clear();
                var newItems = e.NewValue as IEnumerable<MapElement>;
                if (newItems != null)
                {
                    page.MapElements.AddRange(newItems);
                }
            }
        }

        protected object NavigationParameter { get; private set; }

        private bool _shown = false;

        protected List<Tombstoner> Tombstoners
        {
            get
            {
                return _tombstoners;
            }
        }
        private List<Tombstoner> _tombstoners = new List<Tombstoner>();

        public PageBase()
        {
            NavigationCacheMode = NavigationCacheMode.Enabled;

            NavigationHelper = ConstructNavigation();
            NavigationHelper.LoadState += this.NavigationHelper_LoadState;
            NavigationHelper.SaveState += this.NavigationHelper_SaveState;

            MapItems = new ObservableCollection<object>();
            MapItems.CollectionChanged += MapItems_CollectionChanged;
            DataContextChanged += MapContentPage_DataContextChanged;

            MapElements = new ObservableCollection<MapElement>();

            Loaded += (s, e) =>
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
                if (!_shown)
                {
                    _shown = true;
                    Utils.D("OnShowing");
                    OnShowing();
                    Utils.D("OnShown");
                    OnShown();
                }
            };
            Unloaded += (s, e) =>
            {
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
                if (_shown)
                {
                    _shown = false;
                    Utils.D("OnHiding");
                    OnHiding();
                }
                Messenger.Default.Unregister(this);
            };
        }

        void MapItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                var element = item as FrameworkElement;
                if (element != null)
                {
                    element.DataContext = DataContext;
                }
            }
        }

        void MapContentPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            foreach (var item in MapItems)
            {
                var element = item as FrameworkElement;
                if (element != null)
                {
                    element.DataContext = DataContext;
                }
            }
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            OnBackPressed(e);
            NavigationHelper.HardwareButtons_BackPressed(sender, e);
        }

        protected virtual void OnBackPressed(BackPressedEventArgs e) { }

        protected virtual NavigationHelper ConstructNavigation()
        {
            return new NavigationHelper(this);
        }

        protected abstract object ConstructVM(object parameter);

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (e.PageState != null)
            {
                DataContext = e.PageState["DataContext"];
                foreach (var stoner in Tombstoners)
                {
                    stoner.RestoreFrom(e.PageState);
                }
                LoadState(e.PageState);
            }
            else
            {
                DataContext = ConstructVM(NavigationParameter);
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["DataContext"] = DataContext;
            foreach (var stoner in Tombstoners)
            {
                stoner.TombstoneTo(e.PageState);
            }
            SaveState(e.PageState);
        }

        protected virtual void LoadState(IDictionary<string, object> state) { }
        protected virtual void SaveState(IDictionary<string, object> state) { }

        protected virtual void OnShowing() { }
        protected virtual void OnShown() { }
        protected virtual void OnHiding() { }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int)
            {
                object _navigationParameter;
                App.Current.ParamCache.TryGetParam((int)e.Parameter, out _navigationParameter);
                NavigationParameter = _navigationParameter;
            }
            if (!_shown)
            {
                Utils.D("OnShowing");
                OnShowing();
            }
            NavigationHelper.OnNavigatedTo(e);
            if (!_shown)
            {
                _shown = true;
                Utils.D("OnShown");
                OnShown();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (_shown)
            {
                _shown = false;
                Utils.D("OnHiding");
                OnHiding();
            }
            NavigationHelper.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                DataContext = null;
                foreach (var stoner in Tombstoners)
                {
                    stoner.ResetState();
                }
            }
        }
    }
}
