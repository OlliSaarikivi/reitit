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
    public partial class MapFrame : PhoneApplicationFrame
    {
        private TranslateTransform _mapTransform = new TranslateTransform();

        public Map Map { get; private set; }

        public MapFrame()
        {
            InitializeComponent();
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
            _mapTransform.Y = -contentHeight / 2;
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            Map = sender as Map;
            Map.RenderTransform = _mapTransform;
        }
    }
}