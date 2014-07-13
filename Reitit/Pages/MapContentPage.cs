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
        public static DependencyProperty MapHeightProperty = DependencyProperty.Register("MapHeight", typeof(double), typeof(MapContentPage), new PropertyMetadata((double)0));
        public double MapHeight
        {
            get { return (double)this.GetValue(MapHeightProperty); }
            set { this.SetValue(MapHeightProperty, value); }
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

        protected override NavigationHelper ConstructNavigation()
        {
            return new NavigationHelper(this);
        }
    }
}
