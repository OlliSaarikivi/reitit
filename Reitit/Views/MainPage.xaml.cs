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

namespace Reitit
{
    public partial class MainPage : MapFramePage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            DataContext = new MainPageVM();
        }
    }
}