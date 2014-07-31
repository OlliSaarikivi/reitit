using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Reitit
{
    [DataContract]
    class SettingsPageVM : ViewModelBase
    {
        public List<string> WalkingSpeeds
        {
            get { return _walkingSpeeds; }
        }
        [DataMember]
        public List<string> _walkingSpeeds = new List<string>(new string[] {
            Utils.GetString("WalkSpeedSlow"),
            Utils.GetString("WalkSpeedDefault"),
            Utils.GetString("WalkSpeedFast"),
            Utils.GetString("WalkSpeedRunning"),
            Utils.GetString("WalkSpeedCycling"),
        });
        public int SelectedSpeedIndex
        {
            get { return App.Current.Settings.DefaultSpeedIndex; }
            set
            {
                if (value >= 0)
                {
                    App.Current.Settings.DefaultSpeedIndex = value;
                }
            }
        }

        public List<string> RouteTypes
        {
            get { return _routeTypes; }
        }
        [DataMember]
        public List<string> _routeTypes = new List<string>(new string[] {
            Utils.GetString("RouteTypeDefault"),
            Utils.GetString("RouteTypeFastest"),
            Utils.GetString("RouteTypeTransfers"),
            Utils.GetString("RouteTypeWalking"),
        });
        public int SelectedRouteTypeIndex
        {
            get { return App.Current.Settings.DefaultRouteTypeIndex; }
            set
            {
                if (value >= 0)
                {
                    App.Current.Settings.DefaultRouteTypeIndex = value;
                }
            }
        }

        public int TransferMargin
        {
            get { return App.Current.Settings.DefaultTransferMargin; }
            set { App.Current.Settings.DefaultTransferMargin = value; }
        }
    }
}
