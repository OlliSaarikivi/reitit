using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    [DataContract]
    class MapPageVM : ViewModelBase
    {
        public bool ContentMaximized
        {
            get { return _contentMaximized; }
            set { Set(() => ContentMaximized, ref _contentMaximized, value); }
        }
        [DataMember]
        public bool _contentMaximized = true;

        public double ContentMinimizedHeight
        {
            get { return _contentMinimizedHeight; }
            set { Set(() => ContentMinimizedHeight, ref _contentMinimizedHeight, value); }
        }
        [DataMember]
        public double _contentMinimizedHeight;

    }
}
