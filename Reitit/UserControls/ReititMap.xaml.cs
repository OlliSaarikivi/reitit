using Reitit.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Reitit
{
    public class MapItemComparer : IComparer<DependencyObject>
    {
        public static readonly MapItemComparer Instance = new MapItemComparer();

        private MapItemComparer() { }

        public int Compare(DependencyObject x, DependencyObject y)
        {
            var xCoordinate = ReititMap.GetPushpinCoordinate(x);
            double xLatitude = xCoordinate != null ? xCoordinate.Latitude : 0;
            var yCoordinate = ReititMap.GetPushpinCoordinate(y);
            double yLatitude = yCoordinate != null ? yCoordinate.Latitude : 0;
            return xLatitude.CompareTo(yLatitude);
        }
    }

    public class CoerceZoomLevelConverter : LambdaConverter<double, double, object>
    {
        public static double? MaxZoomLevel;
        public static double? MinZoomLevel;

        public CoerceZoomLevelConverter()
            : base((s, p, language) =>
            {
                var max = MaxZoomLevel ?? 20;
                var min = MinZoomLevel ?? 1;
                return s > max ? max : (s < min ? min : s);
            },
            (s, p, language) =>
            {
                return s;
            })
        { }
    }

    public sealed partial class ReititMap : UserControl
    {
        public static ReittiCoordinate GetPushpinCoordinate(DependencyObject obj)
        {
            return (ReittiCoordinate)obj.GetValue(PushpinCoordinateProperty);
        }
        public static void SetPushpinCoordinate(DependencyObject obj, ReittiCoordinate value)
        {
            obj.SetValue(PushpinCoordinateProperty, value);
        }
        public static readonly DependencyProperty PushpinCoordinateProperty =
            DependencyProperty.RegisterAttached("PushpinCoordinate", typeof(ReittiCoordinate), typeof(ReititMap), new PropertyMetadata(null, PushpinCoordinateChanged));
        private static void PushpinCoordinateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var coordinate = e.NewValue as ReittiCoordinate;
            if (coordinate != null)
            {
                var position = new BasicGeoposition { Latitude = coordinate.Latitude, Longitude = coordinate.Longitude };
                if (coordinate.Altitude.HasValue) { position.Altitude = coordinate.Altitude.Value; }
                MapControl.SetLocation(d, new Geopoint(position));
            }
            Action action;
            if (_pushpinCoordinateChangedActions.TryGetValue(d, out action))
            {
                action();
            }
        }
        static Dictionary<DependencyObject, Action> _pushpinCoordinateChangedActions = new Dictionary<DependencyObject, Action>();

        public static bool GetInAutofocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(InAutofocusProperty);
        }
        public static void SetInAutofocus(DependencyObject obj, bool value)
        {
            obj.SetValue(InAutofocusProperty, value);
        }
        public static readonly DependencyProperty InAutofocusProperty =
            DependencyProperty.RegisterAttached("InAutofocus", typeof(bool), typeof(ReititMap), new PropertyMetadata(true, InAutofocusChanged));
        private static void InAutofocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Action action;
            if (_inAutofocusChangedActions.TryGetValue(d, out action))
            {
                action();
            }
        }
        static Dictionary<DependencyObject, Action> _inAutofocusChangedActions = new Dictionary<DependencyObject, Action>();

        public static int GetZLayer(DependencyObject obj)
        {
            return (int)obj.GetValue(ZLayerProperty);
        }
        public static void SetZLayer(DependencyObject obj, int value)
        {
            obj.SetValue(ZLayerProperty, value);
        }
        public static readonly DependencyProperty ZLayerProperty =
            DependencyProperty.RegisterAttached("ZLayer", typeof(int), typeof(ReititMap), new PropertyMetadata(0));

        public ReittiCoordinate Center
        {
            get { return (ReittiCoordinate)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(ReittiCoordinate), typeof(ReititMap), new PropertyMetadata(Utils.DefaultViewCoordinate));

        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }
        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(ReititMap), new PropertyMetadata(Utils.DefaultViewZoom));

        public bool Autofocus
        {
            get { return (bool)GetValue(AutofocusProperty); }
            set { SetValue(AutofocusProperty, value); }
        }
        public static readonly DependencyProperty AutofocusProperty =
            DependencyProperty.Register("Autofocus", typeof(bool), typeof(ReititMap), new PropertyMetadata(true, AutofocusChanged));
        private static void AutofocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ReititMap;
            if (control != null && (bool)e.NewValue)
            {
                control.AutofocusView();
            }
        }

        public double BottomObscuredHeight { get; set; }

        public event Action Touched;

        Dictionary<ObservableCollection<object>, MapItemsRegistration> _registrations = new Dictionary<ObservableCollection<object>, MapItemsRegistration>();
        ObservableCollection<UIElement> _pageMapItems;
        List<Canvas> _mapCanvases;

        public ReititMap()
        {
            this.InitializeComponent();
            _pageMapItems = new ObservableCollection<UIElement>();
            PageLayerItemsControl.ItemsSource = _pageMapItems;
            _mapCanvases = new List<Canvas>();
            CoerceZoomLevelConverter.MaxZoomLevel = Map.MaxZoomLevel;
            CoerceZoomLevelConverter.MinZoomLevel = Map.MinZoomLevel;
        }

        public void UpdateMapTransform(double contentHeight)
        {
            var screenHeight = Window.Current.Bounds.Height;
            Map.TransformOrigin = new Point(0.5, 1 - (double)(screenHeight + contentHeight) / (2 * screenHeight));
            //await Map.TrySetViewAsync(Map.Center.Jiggle(), Map.ZoomLevel, Map.Heading, Map.DesiredPitch, MapAnimationKind.None);
        }

        public void RegisterItems(ObservableCollection<object> items)
        {
            if (!_registrations.ContainsKey(items))
            {
                _registrations.Add(items, new MapItemsRegistration(this, items));
            }
        }

        public void UnregisterItems(ObservableCollection<object> items)
        {
            MapItemsRegistration registration;
            if (_registrations.TryGetValue(items, out registration))
            {
                _registrations.Remove(items);
                registration.Dispose();
            }
        }

        void AddElement(UIElement element)
        {
            InsertElement(element);
            _pushpinCoordinateChangedActions.Add(element, () =>
            {
                _pageMapItems.Remove(element);
                InsertElement(element);
                EnsureOrder();
                if (Autofocus && ReititMap.GetInAutofocus(element))
                {
                    AutofocusView();
                }
            });
            _inAutofocusChangedActions.Add(element, () =>
            {
                if (Autofocus)
                {
                    AutofocusView();
                }
            });
            EnsureOrder();
            if (Autofocus && ReititMap.GetInAutofocus(element))
            {
                AutofocusView();
            }
        }

        void InsertElement(UIElement element)
        {
            int insertIndex = _pageMapItems.UpperBound(element, MapItemComparer.Instance);
            _pageMapItems.Insert(insertIndex, element);
        }

        void RemoveElement(UIElement element)
        {
            _pushpinCoordinateChangedActions.Remove(element);
            _pageMapItems.Remove(element);
            EnsureOrder();
            if (Autofocus && ReititMap.GetInAutofocus(element))
            {
                AutofocusView();
            }
        }

        bool _autofocusDirty = false;
        void AutofocusView()
        {
            _autofocusDirty = true;
            Utils.OnCoreDispatcher(async () =>
            {
                if (_autofocusDirty)
                {
                    await DoAutofocusView();
                    _autofocusDirty = false;
                }
            });
        }

        async Task DoAutofocusView()
        {
            bool atLeastOne = false;
            double maxLatitude = double.MinValue, minLatitude = double.MaxValue, maxLongitude = double.MinValue, minLongitude = double.MaxValue;
            foreach (var element in _pageMapItems)
            {
                if (ReititMap.GetInAutofocus(element))
                {
                    var c = ReititMap.GetPushpinCoordinate(element);
                    if (c != null)
                    {
                        atLeastOne = true;
                        if (c.Latitude > maxLatitude) maxLatitude = c.Latitude;
                        if (c.Latitude < minLatitude) minLatitude = c.Latitude;
                        if (c.Longitude > maxLongitude) maxLongitude = c.Longitude;
                        if (c.Longitude < minLongitude) minLongitude = c.Longitude;
                    }
                }
            }
            if (atLeastOne)
            {
                double latitudeMargin = Math.Max(Utils.MinZoomedLatitudeDiff - (maxLatitude - minLatitude), 0) / 2;
                double longitudeMargin = Math.Max(Utils.MinZoomedLongitudeDiff - (maxLongitude - minLongitude), 0) / 2;
                await Map.TrySetViewBoundsAsync(new GeoboundingBox(
                    new BasicGeoposition
                {
                    Latitude = maxLatitude + latitudeMargin,
                    Longitude = minLongitude - longitudeMargin
                }, new BasicGeoposition
                {
                    Latitude = minLatitude - latitudeMargin,
                    Longitude = maxLongitude + longitudeMargin
                }), new Thickness(20, 50, 20, BottomObscuredHeight + 12), MapAnimationKind.Linear);
            }
        }

        bool _zIndicesDirty = false;
        void EnsureOrder()
        {
            _zIndicesDirty = true;
            Utils.OnCoreDispatcher(() =>
            {
                if (_zIndicesDirty)
                {
                    SetZIndices();
                    _zIndicesDirty = false;
                }
            });
        }

        void SetZIndices()
        {
            if (_mapCanvases.Count == 0)
            {
                _mapCanvases.AddRange(Map.GetChildrenOfType<Canvas>());
            }
            foreach (var canvas in _mapCanvases)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(canvas); ++i)
                {
                    var child = VisualTreeHelper.GetChild(canvas, i) as ContentPresenter;
                    if (child != null)
                    {
                        var content = child.Content as UIElement;
                        if (content != null)
                        {
                            int zLayer = ReititMap.GetZLayer(content);
                            int zIndex = _pageMapItems.IndexOf(content) - zLayer * _pageMapItems.Count;
                            Canvas.SetZIndex(child, -zIndex);
                        }
                    }
                }
            }
        }

        class MapItemsRegistration : IDisposable
        {
            ReititMap _map;
            ObservableCollection<object> _items;
            Dictionary<object, bool> _currentItems = new Dictionary<object, bool>();
            Dictionary<ReititMapItemsControl, ItemsControlRegistration> _subRegistrations = new Dictionary<ReititMapItemsControl, ItemsControlRegistration>();

            public MapItemsRegistration(ReititMap map, ObservableCollection<object> items)
            {
                _map = map;
                _items = items;
                foreach (var item in _items)
                {
                    AddItem(item);
                }
                _items.CollectionChanged += ItemsChanged;
            }

            void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems)
                        {
                            AddItem((DependencyObject)item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems)
                        {
                            RemoveItem((DependencyObject)item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        foreach (var item in e.OldItems)
                        {
                            RemoveItem((DependencyObject)item);
                        }
                        foreach (var item in e.NewItems)
                        {
                            AddItem((DependencyObject)item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (var item in _currentItems.Keys.ToList())
                        {
                            RemoveItem(item);
                        }
                        foreach (var item in _items)
                        {
                            AddItem(item);
                        }
                        break;
                    default:
                        break;
                }
            }

            void AddItem(object item)
            {
                var itemsControl = item as ReititMapItemsControl;
                if (itemsControl != null)
                {
                    if (!_subRegistrations.ContainsKey(itemsControl))
                    {
                        _subRegistrations.Add(itemsControl, new ItemsControlRegistration(_map, itemsControl));
                    }
                }
                else
                {
                    var element = item as UIElement;
                    _map.AddElement(element);
                }
                _currentItems.Add(item, true);
            }

            void RemoveItem(object item)
            {
                var itemsControl = item as ReititMapItemsControl;
                if (itemsControl != null)
                {
                    ItemsControlRegistration subRegistration;
                    if (_subRegistrations.TryGetValue(itemsControl, out subRegistration))
                    {
                        subRegistration.Dispose();
                        _subRegistrations.Remove(itemsControl);
                    }
                }
                else
                {
                    var element = item as UIElement;
                    _map.RemoveElement(element);
                }
                _currentItems.Remove(item);
            }

            public void Dispose()
            {
                _items.CollectionChanged -= ItemsChanged;
                foreach (var item in _items)
                {
                    RemoveItem(item);
                }
            }

            class ItemsControlRegistration : IDisposable
            {
                ReititMap _map;
                ObservableCollection<UIElement> _generatedElements;
                Dictionary<UIElement, bool> _currentElements = new Dictionary<UIElement, bool>();

                public ItemsControlRegistration(ReititMap map, ReititMapItemsControl itemsControl)
                {
                    _map = map;
                    _generatedElements = itemsControl.GeneratedElements;
                    foreach (var element in _generatedElements)
                    {
                        _map.AddElement(element);
                    }
                    _generatedElements.CollectionChanged += GeneratedElementsChanged;
                }

                void GeneratedElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (var item in e.NewItems)
                            {
                                var element = (UIElement)item;
                                _map.AddElement(element);
                                _currentElements.Add(element, true);
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var item in e.OldItems)
                            {
                                var element = (UIElement)item;
                                _map.RemoveElement(element);
                                _currentElements.Remove(element);
                            }
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            foreach (var item in e.OldItems)
                            {
                                var element = (UIElement)item;
                                _map.RemoveElement(element);
                                _currentElements.Remove(element);
                            }
                            foreach (var item in e.NewItems)
                            {
                                var element = (UIElement)item;
                                _map.AddElement(element);
                                _currentElements.Add(element, true);
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            foreach (var element in _currentElements.Keys)
                            {
                                _map.RemoveElement(element);
                            }
                            _currentElements.Clear();
                            foreach (var element in _generatedElements)
                            {
                                _map.AddElement(element);
                                _currentElements.Add(element, true);
                            }
                            break;
                        default:
                            break;
                    }
                }

                public void Dispose()
                {
                    _generatedElements.CollectionChanged -= GeneratedElementsChanged;
                    foreach (var element in _generatedElements)
                    {
                        _map.RemoveElement(element);
                    }
                }
            }
        }

        private void Map_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            var handler = Touched;
            if (handler != null)
            {
                handler();
            }
        }
    }
}
