using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Reitit
{
    public abstract class MapFramePage : PhoneApplicationPage
    {
        public static DependencyProperty MapHeightProperty = DependencyProperty.Register("MapHeight", typeof(double), typeof(MapFramePage), new PropertyMetadata((double)0));
        public double MapHeight
        {
            get { return (double)this.GetValue(MapHeightProperty); }
            set { this.SetValue(MapHeightProperty, value); }
        }

        public List<Tombstoner> Tombstoners
        {
            get
            {
                return _tombstoners;
            }
        }
        private List<Tombstoner> _tombstoners = new List<Tombstoner>();

        private bool _isNewPageInstance = true;

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

        protected abstract object ConstructVM(NavigationEventArgs e);

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isNewPageInstance)
            {
                if (DataContext == null)
                {
                    if (State.Count > 0)
                    {
                        DataContext = State["DataContext"];
                        foreach (var stoner in Tombstoners)
                        {
                            stoner.RestoreFrom(State);
                        }
                    }
                    else
                    {
                        DataContext = ConstructVM(e);
                    }
                }
            }
            _isNewPageInstance = false;

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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                State["DataContext"] = DataContext;
                foreach (var stoner in Tombstoners)
                {
                    stoner.TombstoneTo(State);
                }
            }
        }
    }
}
