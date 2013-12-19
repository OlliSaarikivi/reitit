using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Reitit
{
    class MainPageVM : ObservableObject
    {
        public RelayCommand RouteCommand
        {
            get
            {
                return new RelayCommand(() => { Route(); });
            }
        }

        private void Route()
        {
            App.RootFrame.Navigate(new Uri("/Views/RouteSearchPage.xaml", UriKind.Relative));
        }
    }
}
