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
    [DataContract]
    public class FavIconVM : ExtendedObservableObject
    {
        public string IconName { get { return _iconName; } }
        [DataMember]
        public string _iconName;

        public bool Selected
        {
            get { return _selected; }
            set { Set(() => Selected, ref _selected, value); }
        }
        [DataMember]
        public bool _selected = false;

        public FavIconVM(string iconName)
        {
            _iconName = iconName;
        }
    }

    [DataContract]
    public class EditFavPageVM : ViewModelBase
    {
        public string Title { get { return _title; } }
        [DataMember]
        public string _title;

        public bool IsEdit { get { return _isEdit; } }
        [DataMember]
        public bool _isEdit;

        [DataMember]
        public int _originalId;

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }
        [DataMember]
        public string _name;
        public FavIconVM IconName
        {
            get { return _iconName; }
            set
            {
                if (_iconName != null)
                {
                    _iconName.Selected = false;
                }
                Set(() => IconName, ref _iconName, value);
                if (_iconName != null)
                {
                    _iconName.Selected = true;
                }
            }
        }
        [DataMember]
        public FavIconVM _iconName;
        public IPickerLocation Coordinate
        {
            get { return _coordinate; }
            set { Set(() => Coordinate, ref _coordinate, value); }
        }
        [DataMember]
        public IPickerLocation _coordinate;

        public RelayCommand AcceptCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var favorite = new FavoriteLocation
                    {
                        Name = Name,
                        LocationName = Coordinate.Name,
                        Coordinate = Coordinate.GetCoordinatesSynchronouslyOrNone() ?? Utils.HelsinkiCoordinate,
                        IconName = IconName.IconName,
                    };
                    if (IsEdit)
                    {
                        App.Current.Favorites.ReplaceOrAdd(_originalId, favorite);
                    }
                    else
                    {
                        App.Current.Favorites.Add(favorite);
                    }
                    Messenger.Default.Send(new MapPageGoBackMessage());
                });
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Messenger.Default.Send(new MapPageGoBackMessage());
                });
            }
        }

        public List<FavIconVM> IconNames { get { return _iconNames; } }
        [DataMember]
        public List<FavIconVM> _iconNames = new List<FavIconVM>
        {
            new FavIconVM("FavPoi"),
            new FavIconVM("FavHeart"),
            new FavIconVM("FavCoffee"),
            new FavIconVM("FavFoodbasket"),
            new FavIconVM("FavCocktail"),
            new FavIconVM("FavFastfood"),
            new FavIconVM("FavCake"),
            new FavIconVM("FavBottles"),
            new FavIconVM("FavTrain"),
            new FavIconVM("FavPlane"),
            new FavIconVM("FavAnchor"),
            new FavIconVM("FavCar"),
            new FavIconVM("FavBus"),
            new FavIconVM("FavHouse"),
            new FavIconVM("FavChurch"),
            new FavIconVM("FavFir"),
            new FavIconVM("FavDesk"),
            new FavIconVM("FavLockers"),
            new FavIconVM("FavFarm"),
            new FavIconVM("FavTorso"),
            new FavIconVM("FavCane"),
            new FavIconVM("FavBaby"),
            new FavIconVM("FavParking"),
            new FavIconVM("FavHospital"),
            new FavIconVM("FavUniversity"),
            new FavIconVM("FavLaboratory"),
            new FavIconVM("FavBinoculars"),
            new FavIconVM("FavDog"),
            new FavIconVM("FavGolf"),
            new FavIconVM("FavFishing"),
            new FavIconVM("FavSwimming"),
            new FavIconVM("FavCycling"),
            new FavIconVM("FavSkiing"),
            new FavIconVM("FavStable"),
            new FavIconVM("FavCanoe"),
            new FavIconVM("FavMotorboat"),
            new FavIconVM("FavDigging"),
            new FavIconVM("FavSkateboarding"),
            new FavIconVM("FavWetland"),
        };

        private DerivedProperty<bool> IsValidProperty;
        public bool IsValid { get { return IsValidProperty.Get(); } }

        public EditFavPageVM(FavoriteLocation toEdit = null)
        {
            IsValidProperty = CreateDerivedProperty(() => IsValid,
                () => Coordinate != null && Name != null && Name.Length > 0);

            if (toEdit != null)
            {
                _isEdit = App.Current.Favorites.Contains(toEdit, out _originalId);
                Name = toEdit.Name;
                IconName = IconNames.FirstOrDefault(x => x.IconName == toEdit.IconName)
                    ?? IconNames.FirstOrDefault(x => x.IconName == Utils.DefaultFavIcon)
                    ?? IconNames.FirstOrDefault();
                Coordinate = new MapLocation
                {
                    Coordinate = toEdit.Coordinate,
                    Name = toEdit.LocationName,
                };
            }
            else
            {
                _isEdit = false;
                Name = "";
                IconName = IconNames.FirstOrDefault(x => x.IconName == Utils.DefaultFavIcon)
                    ?? IconNames.FirstOrDefault();
                Coordinate = null;
            }
            _title = Utils.GetString(IsEdit ? "FavEditTitle" : "FavNewTitle");
        }
    }
}
