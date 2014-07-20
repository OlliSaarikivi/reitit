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
                var template = IconTemplateForType(e.NewValue as string);
                icon.Content = template.LoadContent() as UIElement;
            }
        }
        private static DataTemplate IconTemplateForType(string type)
        {
            if (type == "walk")
            {
                return (DataTemplate)App.Current.Resources["WalkIcon"];
            }
            else if (type == "wait")
            {
                return (DataTemplate)App.Current.Resources["WaitIcon"];
            }
            else if (type == "tram" || type == "2")
            {
                return (DataTemplate)App.Current.Resources["TramIcon"];
            }
            else if (type == "metro" || type == "6")
            {
                return (DataTemplate)App.Current.Resources["MetroIcon"];
            }
            else if (type == "ferry" || type == "7")
            {
                return (DataTemplate)App.Current.Resources["FerryIcon"];
            }
            else if (type == "train" || type == "12")
            {
                return (DataTemplate)App.Current.Resources["TrainIcon"];
            }
            else // Buses and everything else
            {
                return (DataTemplate)App.Current.Resources["BusIcon"];
            }
        }

        public ModeIcon()
        {
            this.InitializeComponent();
        }
    }
}
