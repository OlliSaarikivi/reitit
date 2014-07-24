using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Reitit
{
    public abstract class MapContentPage : PageBase
    {
        public double MapHeight
        {
            get { return (double)GetValue(MapHeightProperty); }
            set { SetValue(MapHeightProperty, value); }
        }
        public static readonly DependencyProperty MapHeightProperty =
            DependencyProperty.Register("MapHeight", typeof(double), typeof(MapContentPage), new PropertyMetadata(0.0));

        public double MinimizedHeight { get; set; }

        public bool IsMaximized
        {
            get { return (bool)GetValue(IsMaximizedProperty); }
            set { SetValue(IsMaximizedProperty, value); }
        }
        public static readonly DependencyProperty IsMaximizedProperty =
            DependencyProperty.Register("IsMaximized", typeof(bool), typeof(MapContentPage), new PropertyMetadata(true, IsMaximizedChanged));

        private static void IsMaximizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = d as MapContentPage;
            if (page != null)
            {
                page.OnIsMaximizedChanged();
            }
        }

        public bool ShowMyLocationImplicit
        {
            get { return (bool)GetValue(ShowMyLocationImplicitProperty); }
            set { SetValue(ShowMyLocationImplicitProperty, value); }
        }
        public static readonly DependencyProperty ShowMyLocationImplicitProperty =
            DependencyProperty.Register("ShowMyLocationImplicit", typeof(bool), typeof(MapContentPage), new PropertyMetadata(false, ShowMyLocationImplicitChanged));
        private static void ShowMyLocationImplicitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = d as MapContentPage;
            if (page != null)
            {
                Messenger.Default.Send(new ShowMyLocationImplicitMessage { Show = (bool)e.NewValue }, page);
            }
        }

        public MapContentPage()
        {
            MinimizedHeight = 30;

            // This converter works both ways due to the magical properties of subtraction
            Convert<double, double, object> convert = (otherHeight, P, C) =>
            {
                return Window.Current.Bounds.Height - otherHeight;
            };
            Binding binding = new Binding
            {
                Path = new PropertyPath("Height"),
                Converter = new LambdaConverter<double, double, object>(convert, convert),
                Source = this,
                Mode = BindingMode.TwoWay,
            };
            SetBinding(MapHeightProperty, binding);
        }

        public virtual void OnIsMaximizedChanged() { }

        public virtual void OnMapHolding(FrameworkElement source, ReittiCoordinate coordinate) { }

        protected override NavigationHelper ConstructNavigation()
        {
            return new NavigationHelper(this);
        }
    }
}
