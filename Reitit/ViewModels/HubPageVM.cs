using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Reitit.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Reitit
{
    [DataContract]
    public class MenuItem
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Target { get; set; }
    }

    [DataContract]
    public class HubPageVM : ViewModelBase
    {
        public HubPageVM()
        {
        }

        public ObservableCollection<MenuItem> MenuItems
        {
            get { return _menuItems; }
            set { Set(() => MenuItems, ref _menuItems, value); }
        }
        [DataMember]
        public ObservableCollection<MenuItem> _menuItems = new ObservableCollection<MenuItem> {
            new MenuItem { Title = Utils.GetString("HubRoutesMenuItem"), Target = typeof(RouteSearchPage).FullName },
            new MenuItem { Title = Utils.GetString("HubStopsMenuItem"), Target = typeof(StopSearchPage).FullName },
            new MenuItem { Title = Utils.GetString("HubLinesMenuItem"), Target = typeof(RouteSearchPage).FullName },
        };

        public RelayCommand<ItemClickEventArgs> MenuItemClickedCommand
        {
            get
            {
                return new RelayCommand<ItemClickEventArgs>(e =>
                {
                    var item = e.ClickedItem as MenuItem;
                    if (item != null)
                    {
                        Utils.Navigate(Type.GetType(item.Target));
                    }
                });
            }
        }

        public ObservableCollection<FavoriteLocation> Favorites { get { return App.Current.Favorites.SortedLocations; } }

        public FavoriteLocation HeldFavorite
        {
            get { return _heldFavorite; }
            set { Set(() => HeldFavorite, ref _heldFavorite, value); }
        }
        [DataMember]
        public FavoriteLocation _heldFavorite;

        public RelayCommand EditFavoriteCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (HeldFavorite != null)
                    {
                        Utils.Navigate(typeof(EditFavPage), HeldFavorite);
                    }
                    else
                    {
                        await Utils.ShowOperationFailedError();
                    }
                });
            }
        }

        public RelayCommand DeleteFavoriteCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    if (HeldFavorite != null)
                    {
                        int id;
                        if (App.Current.Favorites.Contains(HeldFavorite, out id))
                        {
                            App.Current.Favorites.Remove(id);
                        }
                        else
                        {
                            await Utils.ShowOperationFailedError();
                        }
                    }
                    else
                    {
                        await Utils.ShowOperationFailedError();
                    }
                });
            }
        }

        public DisruptionsLoader DisruptionsLoader { get { return App.Current.DisruptionsLoader; } }

        public ObservableCollection<Disruption> Disruptions { get { return App.Current.DisruptionsLoader.Disruptions; } }
    }
}
