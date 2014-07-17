using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI;

namespace Reitit
{
    [DataContract]
    public class PushpinVM : ExtendedObservableObject
    {
        public string Label
        {
            get { return _label; }
            set { Set(() => Label, ref _label, value); }
        }
        [DataMember]
        public string _label = "";

        public Geopoint Coordinates
        {
            get { return _coordinates; }
            set { Set(() => Coordinates, ref _coordinates, value); }
        }
        [DataMember]
        public Geopoint _coordinates;

        public Color Color
        {
            get { return _color; }
            set { Set(() => Color, ref _color, value); }
        }
        [DataMember]
        public Color _color = Color.FromArgb(255, 0, 0, 0);

        public bool Visible
        {
            get { return _visible; }
            set { Set(() => Visible, ref _visible, value); }
        }
        [DataMember]
        public bool _visible = true;
    }

    [DataContract]
    public class ReititMapVM : ExtendedObservableObject
    {
        public ObservableCollection<PushpinVM> Pushpins
        {
            get { return _pushpins; }
            set { Set(() => Pushpins, ref _pushpins, value); }
        }
        [DataMember]
        public ObservableCollection<PushpinVM> _pushpins;
    }
}
