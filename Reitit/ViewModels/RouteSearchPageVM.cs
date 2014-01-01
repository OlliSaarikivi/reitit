using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Controls;
using Reitit.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reitit
{
    [KnownType(typeof(FavoritePickerLocation))]
    [KnownType(typeof(MapLocation))]
    [KnownType(typeof(ReittiLocation))]
    [KnownType(typeof(ReittiStopsLocation))]
    [KnownType(typeof(MeLocation))]
    [DataContract]
    public class RouteSearchPageVM : ExtendedObservableObject
    {
        public RouteSearchPageVM()
        {
            SelectedTimeType = TimeTypes[0];
            SelectedSpeed = WalkingSpeeds[0];
            SelectedRouteType = RouteTypes[0];
        }

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
                    var tmp = Time;
                    Time = Date;
                    Date = tmp;
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
        private List<TimeType> _timeTypes = new List<TimeType>(new TimeType[] {
            new TimeType{ Type = "departure", IsTimed = false, Text = AppResources.TimeTypeNow },
            new TimeType{ Type = "departure", IsTimed = true, Text = AppResources.TimeTypeDepartAt },
            new TimeType{ Type = "arrival", IsTimed = true, Text = AppResources.TimeTypeArriveAt },
        });
        public TimeType SelectedTimeType
        {
            get { return _selectedTimeType; }
            set {Set(() => SelectedTimeType, ref _selectedTimeType, value); }
        }
        [DataMember]
        public TimeType _selectedTimeType;
        

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
        private List<Speed> _walkingSpeeds = new List<Speed>(new Speed[] {
            new Speed{ Value = 30, Text = AppResources.WalkSpeedSlow },
            new Speed{ Value = 70, Text = AppResources.WalkSpeedDefault },
            new Speed{ Value = 100, Text = AppResources.WalkSpeedFast },
            new Speed{ Value = 200, Text = AppResources.WalkSpeedRunning },
            new Speed{ Value = 300, Text = AppResources.WalkSpeedCycling },
        });
        public Speed SelectedSpeed
        {
            get { return _selectedSpeed; }
            set { Set(() => SelectedSpeed, ref _selectedSpeed, value); }
        }
        [DataMember]
        public Speed _selectedSpeed;

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
        private List<RouteType> _routeTypes = new List<RouteType>(new RouteType[] {
            new RouteType{ Optimization = "default", Text = AppResources.RouteTypeDefault },
            new RouteType{ Optimization = "fastest", Text = AppResources.RouteTypeFastest },
            new RouteType{ Optimization = "least_transfers", Text = AppResources.RouteTypeTransfers },
            new RouteType{ Optimization = "least_walking", Text = AppResources.RouteTypeWalking },
        });
        public RouteType SelectedRouteType
        {
            get { return _selectedRouteType; }
            set { Set(() => SelectedRouteType, ref _selectedRouteType, value); }
        }
        [DataMember]
        public RouteType _selectedRouteType;

        public int TransferMargin
        {
            get { return _transferMargin; }
            set { Set(() => TransferMargin, ref _transferMargin, value); }
        }
        [DataMember]
        public int _transferMargin;

        public RelayCommand SearchCommand
        {
            get
            {
                return new RelayCommand(async () => await Search() );
            }
        }
        private async Task Search()
        {
        }
    }
}
