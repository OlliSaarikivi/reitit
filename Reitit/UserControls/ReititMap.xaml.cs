using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Reitit
{
    public class MapItemComparer : IComparer<UIElement>
    {
        public static readonly MapItemComparer Instance = new MapItemComparer();

        private MapItemComparer() { }

        public int Compare(UIElement x, UIElement y)
        {
            return MapControl.GetLocation(x).Position.Latitude.CompareTo(MapControl.GetLocation(y).Position.Latitude);
        }
    }

    public sealed partial class ReititMap : UserControl
    {
        public ReititMap()
        {
            this.InitializeComponent();
        }

        public void UpdateMapTransform(double contentHeight)
        {
            var screenHeight = Window.Current.Bounds.Height;
            Map.TransformOrigin = new Point(0.5, 1 - (double)(screenHeight + contentHeight) / (2 * screenHeight));
            //Map.SetView(Map.Center.Copy().DisplaceFrom(Map.Center), Map.ZoomLevel, MapAnimationKind.None);
        }

        public void RegisterItems(ObservableCollection<UIElement> items)
        {

        }

        public void UnregisterItems(ObservableCollection<UIElement> items)
        {

        }
    }
}
