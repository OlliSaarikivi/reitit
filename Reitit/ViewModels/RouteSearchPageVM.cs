using GalaSoft.MvvmLight;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    class RouteSearchPageVM : ObservableObject
    {
        private readonly int NowIndex = 0;
        private readonly int DepartureIndex = 1;
        private readonly int ArrivalIndex = 2;

        public bool DateTimeVisible
        {
            get { return _dateTimeVisible; }
            private set { Set(() => DateTimeVisible, ref _dateTimeVisible, value); }
        }
        private bool _dateTimeVisible;

        public int TimeTypeIndex
        {
            get { return _timeTypeIndex; }
            set
            {
                Set(() => TimeTypeIndex, ref _timeTypeIndex, value);
                DateTimeVisible = (TimeTypeIndex != NowIndex);
            }
        }
        private int _timeTypeIndex;

    }
}
