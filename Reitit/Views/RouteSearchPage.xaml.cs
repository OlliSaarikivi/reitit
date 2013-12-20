using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;

namespace Reitit
{
    public partial class RouteSearchPage : MapFramePage
    {
        public RouteSearchPage()
        {
            InitializeComponent();
            DataContext = new RouteSearchPageVM();
        }

        private void ListPicker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListPicker picker = sender as ListPicker;
            if (picker != null && picker.ListPickerMode == ListPickerMode.Expanded)
            {
                GeneralTransform focusedVisualTransform = picker.TransformToVisual(ContentScroll);
                Rect rectangle = focusedVisualTransform.TransformBounds(new Rect(new Point(picker.Margin.Left, picker.Margin.Top), picker.RenderSize));
                double newOffset = ContentScroll.VerticalOffset + (rectangle.Bottom - ContentScroll.ViewportHeight);
                if (newOffset > ContentScroll.VerticalOffset)
                {
                    ContentScroll.ScrollToVerticalOffset(newOffset);
                }
            }
        }
    }
}