using System;
using System.Collections.Generic;
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

        public double MinimizedHeight
        {
            get { return (double)GetValue(MinimizedHeightProperty); }
            set { SetValue(MinimizedHeightProperty, value); }
        }
        public static readonly DependencyProperty MinimizedHeightProperty =
            DependencyProperty.Register("MinimizedHeight", typeof(double), typeof(MapContentPage), new PropertyMetadata(60.0));

        public bool IsMaximized
        {
            get { return (bool)GetValue(IsMaximizedProperty); }
            set { SetValue(IsMaximizedProperty, value); }
        }
        public static readonly DependencyProperty IsMaximizedProperty =
            DependencyProperty.Register("IsMaximized", typeof(bool), typeof(MapContentPage), new PropertyMetadata(true, IsMaximizedChanged));

        public static void IsMaximizedChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var page = s as MapContentPage;
            if (page != null)
            {
                if ((bool)e.NewValue != (bool)e.OldValue)
                {
                    if ((bool)e.NewValue)
                    {
                        page.OnMaximized();
                    }
                    else
                    {
                        page.OnMinimized();
                    }
                }
            }
        }

        public MapContentPage()
        {
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

        protected virtual void OnMinimized() { }

        protected virtual void OnMaximized() { }

        protected override NavigationHelper ConstructNavigation()
        {
            return new NavigationHelper(this);
        }
    }
}
