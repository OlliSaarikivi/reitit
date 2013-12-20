using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Reitit
{
    public partial class MapFramePage : PhoneApplicationPage
    {
        public static DependencyProperty MapHeightProperty = DependencyProperty.Register("MapHeight", typeof(double), typeof(MapFramePage), new PropertyMetadata((double)0));
        public double MapHeight
        {
            get { return (double)this.GetValue(MapHeightProperty); }
            set { this.SetValue(MapHeightProperty, value); }
        }

        public MapFramePage()
        {
            Binding binding = new Binding("Height");
            Convert<double, double, object> convert = (otherHeight, P, C) =>
            {
                return App.Current.ScaledResolution.Height - otherHeight;
            };
            binding.Converter = new LambdaConverter<double, double, object>(convert, convert);
            binding.Source = this;
            binding.Mode = BindingMode.TwoWay;
            SetBinding(MapHeightProperty, binding);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SystemTray.IsVisible = true;
            SystemTray.Opacity = 0;
            if ((Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible)
            {
                SystemTray.ForegroundColor = (Color)Application.Current.Resources["PhoneForegroundColor"];
            }
            else
            {
                SystemTray.ForegroundColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];
            }
        }
    }
}
