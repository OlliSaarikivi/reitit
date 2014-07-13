using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Reitit
{
    class MenuItem
    {
        public string Title { get; set; }
        public Type Target { get; set; }
    }

    [DataContract]
    class HubPageVM : ViewModelBase
    {
        public RelayCommand<ItemClickEventArgs> MenuItemClickedCommand
        {
            get
            {
                return new RelayCommand<ItemClickEventArgs>(e =>
                {
                    var item = e.ClickedItem as MenuItem;
                    if (item != null)
                    {
                        Utils.Navigate(item.Target);
                    }
                });
            }
        }

        public ObservableCollection<MenuItem> MenuItems
        {
            get { return _menuItems; }
            set { Set(() => MenuItems, ref _menuItems, value); }
        }
        [DataMember]
        public ObservableCollection<MenuItem> _menuItems = new ObservableCollection<MenuItem>(new MenuItem[] {
            new MenuItem { Title = "routes", Target = typeof(RouteSearchPage) },
        });
        
    }
}
