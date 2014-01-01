using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Reitit.Resources;
using System.Device.Location;
using BindableApplicationBar;

namespace Reitit
{
    public partial class MainPage : MapFramePage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override object ConstructVM(NavigationEventArgs e)
        {
            return new MainPageVM();
        }
    }
}