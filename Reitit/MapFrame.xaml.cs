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

        public ReititMap Map { get; private set; }

        public Grid Content2 { get; private set; }

        public Grid Overlay { get; private set; }

        public MapFrame()
        {
            InitializeComponent();

            Overlay = new Grid();
            Map = new ReititMap();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            var oldPage = oldContent as FrameworkElement;
            if (oldPage != null)
            {
                oldPage.SizeChanged -= ContentSizeChanged;
            }

            var page = newContent as MapFramePage;
            if (page != null)
            {
                VerticalContentAlignment = VerticalAlignment.Bottom;
                page.SizeChanged += ContentSizeChanged;
                Map.UpdateMapTransform(page.ActualHeight);
            }
            else
            {
                VerticalContentAlignment = VerticalAlignment.Stretch;
            }
        }

        private void ContentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Map.UpdateMapTransform((sender as FrameworkElement).ActualHeight);
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            Map = sender as ReititMap;
            var content = Content as FrameworkElement;
            if (content != null)
            {
                Map.UpdateMapTransform(content.ActualHeight);
            }
        }

        private void Content_Loaded(object sender, RoutedEventArgs e)
        {
            Content2 = sender as Grid;
            if (!Content2.Children.Contains(Overlay))
            {
                Content2.Children.Insert(0, Map);
                Content2.Children.Add(Overlay);
            }
        }
    }
}