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
        }

        protected override object ConstructVM(NavigationEventArgs e)
        {
            return new RouteSearchPageVM();
        }

        private void ListPicker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((ListPicker)sender).KeepExpandedInView(ContentScroll);
        }

        private void MarginSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MarginSlider.Value = (int)MarginSlider.Value;
        }
    }
}