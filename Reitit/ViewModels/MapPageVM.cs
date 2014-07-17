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
        public ReititMapVM Map { get { return _map; } }
        [DataMember]
        private ReititMapVM _map = new ReititMapVM();

        public bool ContentMaximized
        {
            get { return _contentMaximized; }
            set { Set(() => ContentMaximized, ref _contentMaximized, value); }
        }
        [DataMember]
        public bool _contentMaximized = true;

        public double ContentMinimizedOffset
        {
            get { return _contentMinimizedOffset; }
            set { Set(() => ContentMinimizedOffset, ref _contentMinimizedOffset, value); }
        }
        [DataMember]
        public double _contentMinimizedOffset;

    }
}
