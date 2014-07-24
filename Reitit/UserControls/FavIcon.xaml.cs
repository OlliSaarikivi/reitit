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

namespace Reitit
{
    public sealed partial class FavIcon : UserControl
    {
        public string IconName
        {
            get { return (string)GetValue(IconNameProperty); }
            set { SetValue(IconNameProperty, value); }
        }
        public static readonly DependencyProperty IconNameProperty =
            DependencyProperty.Register("IconName", typeof(string), typeof(FavIcon), new PropertyMetadata(null,IconNameChanged));

        private static void IconNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var icon = d as FavIcon;
            if (icon != null)
            {
                object resource;
                if (!App.Current.Resources.TryGetValue(e.NewValue as string ?? Utils.DefaultFavIcon, out resource))
                {
                    resource = App.Current.Resources[Utils.DefaultFavIcon];
                }
                var template = resource as DataTemplate;
                if (template != null)
                {
                    icon.Viewbox.Child = template.LoadContent() as UIElement;
                }
            }
        }

        public FavIcon()
        {
            this.InitializeComponent();
        }
    }
}
