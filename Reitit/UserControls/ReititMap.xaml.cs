using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class ReititMap : UserControl
    {
        public ReititMap()
        {
            this.InitializeComponent();
        }

        public async Task UpdateMapTransform(double contentHeight)
        {
            var screenHeight = Window.Current.Bounds.Height;
            Map.TransformOrigin = new Point(0.5, 1 - (double)(screenHeight + contentHeight) / (2 * screenHeight));
            await Map.TrySetViewAsync(Map.Center.Jiggle(), Map.ZoomLevel, Map.Heading, Map.DesiredPitch, MapAnimationKind.None);
        }

        private void PushpinStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var panel = (DependencyObject)sender;
            MapControl.SetNormalizedAnchorPoint(panel, new Point(0, 1));
        }
    }
}
