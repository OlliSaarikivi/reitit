using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Reitit
{
    public partial class MapFramePage : PhoneApplicationPage
    {
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
