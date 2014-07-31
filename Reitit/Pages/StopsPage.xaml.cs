using Reitit.API;
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
    public sealed partial class StopsPage : MapContentPage
    {
        public StopsPage()
        {
            this.InitializeComponent();
        }

        protected override object ConstructVM(object parameter)
        {
            var stops = parameter as IEnumerable<ConnectedStops>;
            return new StopsPageVM(stops);
        }
    }
}
