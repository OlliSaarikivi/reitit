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
            double xLatitude = 0;
            try
            {
                xLatitude = ReititMap.GetPushpinCoordinate(x).Latitude;
            }
            catch (NullReferenceException) { }
            double yLatitude = 0;
            try
            {
                yLatitude = ReititMap.GetPushpinCoordinate(y).Latitude;
            }
            catch (NullReferenceException) { }
            return xLatitude.CompareTo(yLatitude);
        }
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
            DependencyProperty.RegisterAttached("PushpinCoordinate", typeof(ReittiCoordinate), typeof(ReititMap), new PropertyMetadata(null, PushpinCoordinate));

        private static void PushpinCoordinate(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

        Dictionary<ObservableCollection<object>, MapItemsRegistration> _registrations = new Dictionary<ObservableCollection<object>, MapItemsRegistration>();
        //ObservableCollection<UIElement> _pageMapItems;

        public ReititMap()
        {
            this.InitializeComponent();
            //_pageMapItems = new ObservableCollection<UIElement>();
            //PageLayerItemsControl.ItemsSource = _pageMapItems;
        }

        public async Task UpdateMapTransform(double contentHeight)
        {
            var screenHeight = Window.Current.Bounds.Height;
            Map.TransformOrigin = new Point(0.5, 1 - (double)(screenHeight + contentHeight) / (2 * screenHeight));
            await Map.TrySetViewAsync(Map.Center.Jiggle(), Map.ZoomLevel, Map.Heading, Map.DesiredPitch, MapAnimationKind.None);
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
                PageLayerItemsControl.Items.Clear();
                PageLayerItemsControl.Items.Remove(element);
                InsertElement(element);
            });
            MapControl.SetNormalizedAnchorPoint(element, new Point(0, 1));
        }

        void InsertElement(UIElement element)
        {
            int insertIndex = PageLayerItemsControl.Items.UpperBound(element, MapItemComparer.Instance);
            if (insertIndex == PageLayerItemsControl.Items.Count)
            {
                PageLayerItemsControl.Items.Add(element);
            }
            else
            {
                PageLayerItemsControl.Items.Insert(insertIndex, element);
            }
        }

        void RemoveElement(UIElement element)
        {
            _pushpinCoordinateChangedActions.Remove(element);
            PageLayerItemsControl.Items.Remove(element);
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
                    var element = item as ReititMapItem;
                    _map.AddElement(element.Element);
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
                    var element = item as ReititMapItem;
                    _map.RemoveElement(element.Element);
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
    }
}
