using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    [DataContract]
    public abstract class ViewModelBase : ExtendedObservableObject
    {
        public ViewModelBase()
        {
            Initialize();
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            Initialize();
        }

        protected abstract void Initialize();
    }
}
