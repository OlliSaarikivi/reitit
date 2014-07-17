﻿using GalaSoft.MvvmLight;
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
using System.Windows;
using Windows.Devices.Geolocation;
using Windows.UI;

namespace Reitit
{
    [KnownType(typeof(FavoritePickerLocation))]
    [KnownType(typeof(MapLocation))]
    [KnownType(typeof(ReittiLocation))]
    [KnownType(typeof(ReittiStopsLocation))]
    [KnownType(typeof(MeLocation))]
    [DataContract]
    public class RouteSearchPageVM : ViewModelBase
    {
        static RouteSearchPageVM() { SuspensionManager.KnownTypes.Add(typeof(RouteSearchPageVM)); }
        protected override void Initialize()
        {
            SelectedTimeTypeProperty = CreateDerivedProperty(
                () => SelectedTimeType,
                () => TimeTypes[SelectedTimeTypeIndex]);
            SelectedSpeedProperty = CreateDerivedProperty(
                () => SelectedSpeed,
                () => WalkingSpeeds[SelectedSpeedIndex]);
            SelectedRouteTypeProperty = CreateDerivedProperty(
                () => SelectedRouteType,
                () => RouteTypes[SelectedRouteTypeIndex]);
            SearchPossibleProperty = CreateDerivedProperty(
                () => SearchPossible,
                () => From != null && To != null);
        }

        private DerivedProperty<bool> SearchPossibleProperty;
        public bool SearchPossible { get { return SearchPossibleProperty.Get(); } }

        public IPickerLocation From
        {
            get { return _from; }
            set { Set(() => From, ref _from, value); }
        }
        [DataMember]
        public IPickerLocation _from = MeLocation.Instance;

        public IPickerLocation To
        {
            get { return _to; }
            set { Set(() => To, ref _to, value); }
        }
        [DataMember]
        public IPickerLocation _to;

        public DateTime Time
        {
            get { return _time; }
            set { Set(() => Time, ref _time, value); }
        }
        [DataMember]
        public DateTime _time = DateTime.Now;

        public DateTime Date
        {
            get { return _date; }
            set { Set(() => Date, ref _date, value); }
        }
        [DataMember]
        public DateTime _date = DateTime.Now;

        public RelayCommand SwapFromToCommand
        {
            get
            { 
                return new RelayCommand(() =>
                {
                    var tmp = From;
                    From = To;
                    To = tmp;
                    return;
                });
            }
        }

        [DataContract]
        public class TimeType
        {
            [DataMember]
            public string Text { get; set; }
            [DataMember]
            public string Type { get; set; }
            [DataMember]
            public bool IsTimed { get; set; }
        }
        public List<TimeType> TimeTypes
        {
            get { return _timeTypes; }
        }
        [DataMember]
        public List<TimeType> _timeTypes = new List<TimeType>(new TimeType[] {
            new TimeType{ Type = "departure", IsTimed = false, Text = Utils.GetString("TimeTypeNow") },
            new TimeType{ Type = "departure", IsTimed = true, Text = Utils.GetString("TimeTypeDepartAt") },
            new TimeType{ Type = "arrival", IsTimed = true, Text = Utils.GetString("TimeTypeArriveAt") },
        });
        public int SelectedTimeTypeIndex
        {
            get { return _selectedTimeTypeIndex; }
            set { Set(() => SelectedTimeTypeIndex, ref _selectedTimeTypeIndex, value); }
        }
        [DataMember]
        public int _selectedTimeTypeIndex;
        private DerivedProperty<TimeType> SelectedTimeTypeProperty;
        public TimeType SelectedTimeType
        {
            get { return SelectedTimeTypeProperty.Get(); }
        }
        
        public bool UseBus
        {
            get { return _useBus; }
            set { Set(() => UseBus, ref _useBus, value); }
        }
        [DataMember]
        public bool _useBus = true;

        public bool UseMetro
        {
            get { return _useMetro; }
            set { Set(() => UseMetro, ref _useMetro, value); }
        }
        [DataMember]
        public bool _useMetro = true;

        public bool UseTram
        {
            get { return _useTram; }
            set { Set(() => UseTram, ref _useTram, value); }
        }
        [DataMember]
        public bool _useTram = true;

        public bool UseTrain
        {
            get { return _useTrain; }
            set { Set(() => UseTrain, ref _useTrain, value); }
        }
        [DataMember]
        public bool _useTrain = true;

        [DataContract]
        public class Speed
        {
            [DataMember]
            public string Text { get; set; }
            [DataMember]
            public int Value { get; set; }
        }
        public List<Speed> WalkingSpeeds
        {
            get { return _walkingSpeeds; }
        }
        [DataMember]
        public List<Speed> _walkingSpeeds = new List<Speed>(new Speed[] {
            new Speed{ Value = 30, Text = Utils.GetString("WalkSpeedSlow") },
            new Speed{ Value = 70, Text = Utils.GetString("WalkSpeedDefault") },
            new Speed{ Value = 100, Text = Utils.GetString("WalkSpeedFast") },
            new Speed{ Value = 200, Text = Utils.GetString("WalkSpeedRunning") },
            new Speed{ Value = 300, Text = Utils.GetString("WalkSpeedCycling") },
        });
        public int SelectedSpeedIndex
        {
            get { return _selectedSpeedIndex; }
            set { Set(() => SelectedSpeedIndex, ref _selectedSpeedIndex, value); }
        }
        [DataMember]
        public int _selectedSpeedIndex;
        private DerivedProperty<Speed> SelectedSpeedProperty;
        public Speed SelectedSpeed
        {
            get { return SelectedSpeedProperty.Get(); }
        }

        [DataContract]
        public class RouteType
        {
            [DataMember]
            public string Text { get; set; }
            [DataMember]
            public string Optimization { get; set; }
        }
        public List<RouteType> RouteTypes
        {
            get { return _routeTypes; }
        }
        [DataMember]
        public List<RouteType> _routeTypes = new List<RouteType>(new RouteType[] {
            new RouteType{ Optimization = "default", Text = Utils.GetString("RouteTypeDefault") },
            new RouteType{ Optimization = "fastest", Text = Utils.GetString("RouteTypeFastest") },
            new RouteType{ Optimization = "least_transfers", Text = Utils.GetString("RouteTypeTransfers") },
            new RouteType{ Optimization = "least_walking", Text = Utils.GetString("RouteTypeWalking") },
        });
        public int SelectedRouteTypeIndex
        {
            get { return _selectedRouteTypeIndex; }
            set { Set(() => SelectedRouteTypeIndex, ref _selectedRouteTypeIndex, value); }
        }
        [DataMember]
        public int _selectedRouteTypeIndex;
        private DerivedProperty<RouteType> SelectedRouteTypeProperty;
        public RouteType SelectedRouteType
        {
            get { return SelectedRouteTypeProperty.Get(); }
        }

        public int TransferMargin
        {
            get { return _transferMargin; }
            set { Set(() => TransferMargin, ref _transferMargin, value); }
        }
        [DataMember]
        public int _transferMargin;

        //public bool ShowAdvanced
        //{
        //    get { return _showAdvanced; }
        //    set { Set(() => ShowAdvanced, ref _showAdvanced, value); }
        //}
        //[DataMember]
        //public bool _showAdvanced;

        //public RelayCommand ShowAdvancedCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            ShowAdvanced = true;
        //            Messenger.Default.Send(new AnimateAdvanced());
        //        });
        //    }
        //}

        public PushpinVM FromPushpin = new PushpinVM
        {
            Color = Utils.FromColor,
        };

        public ObservableCollection<PushpinVM> Pushpins { get { return _pushpins; } }
        [DataMember]
        private ObservableCollection<PushpinVM> _pushpins = new ObservableCollection<PushpinVM>(new PushpinVM[] {
            new PushpinVM { Coordinates = new Geopoint(new BasicGeoposition { Latitude = 60, Longitude = 24 }) }
        });

        public RelayCommand SearchCommand
        {
            get
            {
                return new RelayCommand(() => Search());
            }
        }

        private void Search()
        {
            IEnumerable<string> transportTypes;
            if (UseBus && UseMetro && UseTram && UseTrain)
            {
                transportTypes = new string[] { "all" };
            }
            else
            {
                var transportTypesList = new List<string>();
                transportTypesList.Add("ferry");
                if (UseBus)
                {
                    transportTypesList.Add("bus");
                    transportTypesList.Add("service");
                    transportTypesList.Add("uline");
                }
                if (UseMetro)
                {
                    transportTypesList.Add("metro");
                }
                if (UseTram)
                {
                    transportTypesList.Add("tram");
                }
                if (UseTrain)
                {
                    transportTypesList.Add("train");
                }
                transportTypes = transportTypesList;
            }

            var parameters = new RouteSearchParameters
            {
                From = From,
                To = To,
                DateTime = SelectedTimeType.IsTimed ? Date.AddHours(Time.Hour).AddMinutes(Time.Minute) : DateTime.Now,
                Timetype = SelectedTimeType.Type,
                Optimize = SelectedRouteType.Optimization,
                ChangeMargin = TransferMargin,
                WalkSpeed = SelectedSpeed.Value,
            };
            parameters.TransportTypes.AddRange(transportTypes);

            var loader = new RouteLoader(parameters);
            Utils.Navigate(typeof(RoutesPage), loader);
        }
    }
}
