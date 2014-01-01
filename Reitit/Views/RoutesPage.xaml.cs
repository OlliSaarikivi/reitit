using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ReittiAPI;

namespace Reitit.Views
{
    public partial class RoutesPage : MapFramePage
    {
        public RoutesPage()
        {
            InitializeComponent();
        }

        protected override object ConstructVM(NavigationEventArgs e)
        {
            var searchParameters = (RouteSearchParameters)App.Current.Parameters.RemoveParam(NavigationContext.QueryString, "search");
            var routes = (List<CompoundRoute>)App.Current.Parameters.RemoveParam(NavigationContext.QueryString, "routes");
            return new RoutesPageVM(searchParameters, routes);
        }
    }
}