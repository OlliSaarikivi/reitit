using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Reitit
{
    public sealed partial class ModeIcon : UserControl
    {
        public string Type
        {
            get { return (string)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(string), typeof(ModeIcon), new PropertyMetadata("", TypeChanged));

        private static void TypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var icon = d as ModeIcon;
            if (icon != null)
            {
                var template = IconTemplateForType(e.NewValue as string ?? "");
                if (template != null)
                {
                    icon.Content = template.LoadContent() as UIElement;
                }
            }
        }
        private static DataTemplate IconTemplateForType(string type)
        {
            if (type == "walk")
            {
                return App.Current.Resources["WalkIcon"] as DataTemplate;
            }
            else if (type == "wait")
            {
                return App.Current.Resources["WaitIcon"] as DataTemplate;
            }
            else if (type == "tram" || type == "2")
            {
                return App.Current.Resources["TramIcon"] as DataTemplate;
            }
            else if (type == "metro" || type == "6")
            {
                return App.Current.Resources["MetroIcon"] as DataTemplate;
            }
            else if (type == "ferry" || type == "7")
            {
                return App.Current.Resources["FerryIcon"] as DataTemplate;
            }
            else if (type == "train" || type == "12")
            {
                return App.Current.Resources["TrainIcon"] as DataTemplate;
            }
            else // Buses and everything else
            {
                return App.Current.Resources["BusIcon"] as DataTemplate;
            }
        }

        public ModeIcon()
        {
            this.InitializeComponent();
        }
    }
}
