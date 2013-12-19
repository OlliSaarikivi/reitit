using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Media;

namespace Reitit
{
    public partial class MapFrame : TransitionFrame
    {
        private readonly int _screen_height;

        public Map Map { get; private set; }

        public MapFrame()
        {
            InitializeComponent();

            var content = Application.Current.Host.Content;
            _screen_height = (int)Math.Ceiling((double)(content.ActualHeight * content.ScaleFactor) / 100);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            var oldPage = oldContent as FrameworkElement;
            if (oldPage != null)
            {
                oldPage.SizeChanged -= ContentSizeChanged;
            }

            var page = newContent as FrameworkElement;
            if (page != null)
            {
                page.SizeChanged += ContentSizeChanged;
                UpdateMapTransform(page.ActualHeight);
            }
            else
            {
                throw new NotImplementedException("MapFrame does not support non-FrameworkElement content");
            }
        }

        private void ContentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMapTransform((sender as FrameworkElement).ActualHeight);
        }

        private void UpdateMapTransform(double contentHeight)
        {
            if (Map != null)
            {
                Map.TransformCenter = new Point(0.5, 1 - (double)(_screen_height + contentHeight) / (2 * _screen_height));
                Map.SetView(Map.Center.Copy().DisplaceFrom(Map.Center), Map.ZoomLevel, MapAnimationKind.Linear);
            }
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            Map = sender as Map;
            var content = Content as FrameworkElement;
            if (content != null)
            {
                UpdateMapTransform(content.ActualHeight);
            }
        }
    }
}