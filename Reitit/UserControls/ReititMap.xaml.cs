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
using Windows.System.Threading;
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
        public Point CoordinateToAngledPoint(ReittiCoordinate coordinate)
        {
            var point = coordinate.ToUniformPoint();
            var angle = (Map.Heading / 180) * Math.PI;
            var x = point.X * Math.Cos(angle) - point.Y * Math.Sin(angle);
            var y = point.X * Math.Sin(angle) + point.Y * Math.Cos(angle);
            return new Point(x, y);
        }

        public MapControl Map { get; set; }

        public int Compare(DependencyObject a, DependencyObject b)
        {
            var aCoordinate = ReititMap.GetPushpinCoordinate(a) ?? ReittiCoordinate.Zero;
            double aY = CoordinateToAngledPoint(aCoordinate).Y;
            var bCoordinate = ReititMap.GetPushpinCoordinate(b) ?? ReittiCoordinate.Zero;
            double bY = CoordinateToAngledPoint(bCoordinate).Y;
            return aY.CompareTo(bY);
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

        private double _contentHeight;

        public event Action<ReittiCoordinate, Point> ReititHolding;

        MapItemComparer _itemComparer;
        Dictionary<ObservableCollection<object>, MapItemsRegistration> _itemRegistrations = new Dictionary<ObservableCollection<object>, MapItemsRegistration>();
        List<FrameworkElement> _sortedMapItems = new List<FrameworkElement>();
        List<FrameworkElement> _focusers = new List<FrameworkElement>();

        Dictionary<ObservableCollection<MapElement>, MapElementsRegistration> _elementRegistrations = new Dictionary<ObservableCollection<MapElement>, MapElementsRegistration>();

        public ReititMap()
        {
            this.InitializeComponent();
            _itemComparer = new MapItemComparer { Map = Map };
            CoerceZoomLevelConverter.MaxZoomLevel = Map.MaxZoomLevel;
            CoerceZoomLevelConverter.MinZoomLevel = Map.MinZoomLevel;
            Map.Loaded += (s, e) =>
            {
                if (Autofocus)
                {
                    AutofocusView();
                }
            };
        }

        public void UpdateMapTransform(double contentHeight)
        {
            _contentHeight = contentHeight;
            var screenHeight = Window.Current.Bounds.Height;
            var point = new Point(0.5, 1 - (double)(screenHeight + contentHeight) / (2 * screenHeight));
            if (point.Y < 0.001)
            {
                point.Y = 0.001;
            }
            Map.TransformOrigin = point;
        }

        public async Task SetView(ReittiCoordinate center, double zoomLevel)
        {
            if (!await Map.TrySetViewAsync(center, zoomLevel, 0, null, MapAnimationKind.Bow))
            {
                await Map.TrySetViewAsync(center, zoomLevel, 0, 0, MapAnimationKind.Bow);
            }
        }

        private void Map_MapHolding(MapControl sender, MapInputEventArgs args)
        {
            var handler = ReititHolding;
            if (handler != null)
            {
                handler((ReittiCoordinate)args.Location, args.Position);
            }
        }

        #region Ordering

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

        private IEnumerable<FrameworkElement> FocusList()
        {
            foreach (var element in _sortedMapItems)
            {
                yield return element;
            }
            foreach (var element in _focusers)
            {
                yield return element;
            }
        }

        public async Task DoAutofocusView()
        {
            bool atLeastOne = false;
            double maxLatitude = double.MinValue, minLatitude = double.MaxValue, maxLongitude = double.MinValue, minLongitude = double.MaxValue;
            foreach (var element in FocusList())
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
                var bounds = new GeoboundingBox(
                    new BasicGeoposition
                {
                    Latitude = maxLatitude + latitudeMargin,
                    Longitude = minLongitude - longitudeMargin
                }, new BasicGeoposition
                {
                    Latitude = minLatitude - latitudeMargin,
                    Longitude = maxLongitude + longitudeMargin
                });
                var margin = new Thickness(50, 50, 50, _contentHeight + 5);
                var viewSet = await Map.TrySetViewBoundsAsync(bounds, margin, MapAnimationKind.Linear);
                if (!viewSet)
                {
                    Map.DesiredPitch = 0;
                    Map.Heading = 0;
                    await Map.TrySetViewBoundsAsync(bounds, margin, MapAnimationKind.Linear);
                }
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

        private async void Map_HeadingChanged(MapControl sender, object args)
        {
            await SortDelayed();
        }

        private Task _sortTask;
        private async Task SortDelayed()
        {
            if (_sortTask == null)
            {
                _sortTask = Task.Delay(TimeSpan.FromSeconds(0.05)).ContinueWith(async s =>
                {
                    await Utils.OnCoreDispatcher(() =>
                    {
                        _sortTask = null;
                        _sortedMapItems.Sort(_itemComparer);
                        EnsureOrder();
                    });
                });
            }
            await _sortTask;
        }

        void SetZIndices()
        {
            foreach (var c in Map.GetChildrenOfType<ContentPresenter>())
            {
                var child = c.Content as FrameworkElement;
                if (child != null)
                {
                    int i = _sortedMapItems.IndexOf(child);
                    if (i >= 0)
                    {
                        int zLayer = ReititMap.GetZLayer(_sortedMapItems[i]);
                        int zIndex = i - zLayer * _sortedMapItems.Count;
                        Canvas.SetZIndex(c, -zIndex);
                    }
                }
            }
            SetFlippings();
        }

        void SetFlippings()
        {
            var flippables = new List<IFlippable>(from x in _sortedMapItems
                                                  where x is IFlippable && Flippable.GetParticipatesInFlipping(x)
                                                  select x as IFlippable);
            int n = flippables.Count;

            // Calculate what to avoid
            var direction = new double[n];
            var avoidEast = new List<int>[n];
            var avoidWest = new List<int>[n];
            for (int i = 0; i < n; ++i)
            {
                var iCoord = ReititMap.GetPushpinCoordinate(flippables[i] as FrameworkElement);
                if (iCoord != null)
                {
                    var iPoint = _itemComparer.CoordinateToAngledPoint(iCoord);
                    for (int j = i + 1; j < n; ++j)
                    {
                        var jCoord = ReititMap.GetPushpinCoordinate(flippables[j] as FrameworkElement);
                        if (jCoord == null)
                        {
                            break;
                        }
                        var jPoint = _itemComparer.CoordinateToAngledPoint(jCoord);
                        if (Math.Abs(iPoint.Y - jPoint.Y) > Utils.PushpinAvoidDiffY)
                        {
                            break;
                        }
                        double xDiff = jPoint.X - iPoint.X;
                        int eastern = xDiff > 0 ? j : i;
                        int western = xDiff > 0 ? i : j;
                        if (Math.Abs(xDiff) < Utils.PushpinAvoidDiffX)
                        {
                            direction[eastern] += flippables[western].Importance;
                            direction[western] -= flippables[eastern].Importance;
                        }
                        else if (Math.Abs(xDiff) < Utils.PushpinAvoidDiffX * 2)
                        {
                            if (avoidWest[eastern] == null) { avoidWest[eastern] = new List<int>(); };
                            avoidWest[eastern].Add(western);
                            if (avoidEast[western] == null) { avoidEast[western] = new List<int>(); };
                            avoidEast[western].Add(eastern);
                        }
                    }
                }
            }

            // Greedy algorithm for avoidance
            for (int i = 0; i < n; ++i)
            {
                if (avoidWest[i] != null)
                {
                    foreach (var toWest in avoidWest[i])
                    {
                        if (direction[toWest] >= 0)
                        {
                            direction[i] += flippables[toWest].Importance / 1000.0;
                        }
                    }
                }
                if (avoidEast[i] != null)
                {
                    foreach (var toEast in avoidEast[i])
                    {
                        if (direction[toEast] < 0)
                        {
                            direction[i] -= flippables[toEast].Importance / 1000.0;
                        }
                    }
                }
                flippables[i].SetFlip(direction[i] < 0 ? FlipPreference.West : FlipPreference.East);
            }
        }

        #endregion

        #region Items

        public void RegisterItems(ObservableCollection<object> items)
        {
            if (!_itemRegistrations.ContainsKey(items))
            {
                _itemRegistrations.Add(items, new MapItemsRegistration(this, items));
            }
        }

        public void UnregisterItems(ObservableCollection<object> items)
        {
            MapItemsRegistration registration;
            if (_itemRegistrations.TryGetValue(items, out registration))
            {
                _itemRegistrations.Remove(items);
                registration.Dispose();
            }
        }

        void AddElement(FrameworkElement element)
        {
            if (element is ReititMapFocuser)
            {
                _focusers.Add(element);
            }
            else
            {
                InsertElement(element);
            }
            _pushpinCoordinateChangedActions.Add(element, async () =>
            {
                await SortDelayed();
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

        void InsertElement(FrameworkElement element)
        {
            Map.Children.Add(element);
            int insertIndex = _sortedMapItems.UpperBound(element, _itemComparer);
            _sortedMapItems.Insert(insertIndex, element);
        }

        void RemoveElement(FrameworkElement element)
        {
            _pushpinCoordinateChangedActions.Remove(element);
            _inAutofocusChangedActions.Remove(element);
            if (element is ReititMapFocuser)
            {
                _focusers.Remove(element);
            }
            else
            {
                Map.Children.Remove(element);
                _sortedMapItems.Remove(element);
            }
            EnsureOrder();
            if (Autofocus && ReititMap.GetInAutofocus(element))
            {
                AutofocusView();
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
                    var itemPresenter = item as ReititMapItemPresenter;
                    if (itemPresenter != null)
                    {
                        _map.AddElement(itemPresenter.GenerateElement());
                    }
                    else
                    {
                        var element = item as FrameworkElement;
                        if (element != null)
                        {
                            _map.AddElement(element);
                        }
                        else
                        {
                            // Not a supported type
                            return;
                        }
                    }
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
                    var itemPresenter = item as ReititMapItemPresenter;
                    if (itemPresenter != null)
                    {
                        _map.RemoveElement(itemPresenter.CurrentElement);
                    }
                    else
                    {
                        var element = item as FrameworkElement;
                        if (element != null)
                        {
                            _map.RemoveElement(element);
                        }
                        else
                        {
                            // Not a supported type
                            return;
                        }
                    }
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
                ReititMapItemsControl _itemsControl;
                ObservableCollection<FrameworkElement> _generatedElements;
                Dictionary<FrameworkElement, bool> _currentElements = new Dictionary<FrameworkElement, bool>();

                public ItemsControlRegistration(ReititMap map, ReititMapItemsControl itemsControl)
                {
                    _map = map;
                    _itemsControl = itemsControl;
                    _itemsControl.RegenerateIfNecessary();
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
                                var element = (FrameworkElement)item;
                                _map.AddElement(element);
                                _currentElements.Add(element, true);
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var item in e.OldItems)
                            {
                                var element = (FrameworkElement)item;
                                _map.RemoveElement(element);
                                _currentElements.Remove(element);
                            }
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            foreach (var item in e.OldItems)
                            {
                                var element = (FrameworkElement)item;
                                _map.RemoveElement(element);
                                _currentElements.Remove(element);
                            }
                            foreach (var item in e.NewItems)
                            {
                                var element = (FrameworkElement)item;
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
                    _itemsControl.MarkDirty();
                }
            }
        }
        #endregion

        #region Elements

        public void RegisterMapElements(ObservableCollection<MapElement> elements)
        {
            if (!_elementRegistrations.ContainsKey(elements))
            {
                _elementRegistrations.Add(elements, new MapElementsRegistration(this, elements));
            }
        }

        public void UnregisterMapElements(ObservableCollection<MapElement> elements)
        {
            MapElementsRegistration registration;
            if (_elementRegistrations.TryGetValue(elements, out registration))
            {
                _elementRegistrations.Remove(elements);
                registration.Dispose();
            }
        }

        void AddMapElement(MapElement element)
        {
            Map.MapElements.Add(element);
        }

        void RemoveMapElement(MapElement element)
        {
            Map.MapElements.Remove(element);
        }

        class MapElementsRegistration : IDisposable
        {
            ReititMap _map;
            ObservableCollection<MapElement> _elements;
            Dictionary<MapElement, bool> _currentElements = new Dictionary<MapElement, bool>();

            public MapElementsRegistration(ReititMap map, ObservableCollection<MapElement> elements)
            {
                _map = map;
                _elements = elements;
                foreach (var element in _elements)
                {
                    AddElement(element);
                }
                _elements.CollectionChanged += ElementsChanged;
            }

            void ElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems)
                        {
                            AddElement((MapElement)item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems)
                        {
                            RemoveElement((MapElement)item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        foreach (var item in e.OldItems)
                        {
                            RemoveElement((MapElement)item);
                        }
                        foreach (var item in e.NewItems)
                        {
                            AddElement((MapElement)item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (var item in _currentElements.Keys.ToList())
                        {
                            RemoveElement(item);
                        }
                        foreach (var item in _elements)
                        {
                            AddElement(item);
                        }
                        break;
                    default:
                        break;
                }
            }

            void AddElement(object item)
            {
                var element = item as MapElement;
                _map.AddMapElement(element);
                _currentElements.Add(element, true);
            }

            void RemoveElement(object item)
            {
                var element = item as MapElement;
                _map.RemoveMapElement(element);
                _currentElements.Remove(element);
            }

            public void Dispose()
            {
                _elements.CollectionChanged -= ElementsChanged;
                foreach (var item in _elements)
                {
                    RemoveElement(item);
                }
            }
        }

        #endregion
    }
}
