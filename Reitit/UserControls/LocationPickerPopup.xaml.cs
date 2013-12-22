using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Reitit
{
    public partial class LocationPickerPopup : LocationPickerPopupBase
    {
        public LocationPickerPopup()
        {
            InitializeComponent();

            DoneButton.Command = new RelayCommand(() =>
            {
                throw new NotImplementedException();
            });
            CancelButton.Command = new RelayCommand(() =>
            {
                Done(null);
            });
        }
        protected override void InitializeWithCurrent(IPickerLocation current)
        {
            DataContext = new LocationPickerPopupVM();
        }
    }

    class LocationPickerPopupVM : ObservableObject
    {
        public string SearchTerm
        {
            get { return _searchTerm; }
            set { Set(() => SearchTerm, ref _searchTerm, value); }
        }
        private string _searchTerm;

        public bool NoResultsVisible
        {
            get { return _noResultsVisible; }
            set { Set(() => NoResultsVisible, ref _noResultsVisible, value); }
        }
        private bool _noResultsVisible;

        public object SearchTag
        {
            get { return _searchTag; }
        }
        private static object _searchTag = new object();
        public object MapTag
        {
            get { return _mapTag; }
        }
        private static object _mapTag = new object();
        public object FavoritesTag
        {
            get { return _favoritesTag; }
        }
        private static object _favoritesTag = new object();
    }
}
