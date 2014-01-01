using GalaSoft.MvvmLight;
using ReittiAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    class RoutesPageVM : ObservableObject
    {
        public RouteSearchParameters SearchParameters { get; private set; }
        public ObservableCollection<CompoundRoute> Routes { get { return _routes; } }
        private ObservableCollection<CompoundRoute> _routes = new ObservableCollection<CompoundRoute>();

        public RoutesPageVM(ReittiAPI.RouteSearchParameters searchParameters, List<CompoundRoute> routes)
        {
            SearchParameters = searchParameters;
            Routes.AddRange(routes);
        }
    }
}
