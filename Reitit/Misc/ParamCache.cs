using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    public class ParamCache
    {
        private Dictionary<int, object> _leases = new Dictionary<int, object>();

        private int GetParamId()
        {
            var current = App.Current.Settings.ParamCacheNextId;
            App.Current.Settings.ParamCacheNextId = current + 1;
            return current;
        }

        public int AddParam(object param)
        {
            int id = GetParamId();
            _leases.Add(id, param);

            return id;
        }

        public int ReserveId()
        {
            return GetParamId();
        }

        public bool ReleaseParam(int id)
        {
            return _leases.Remove(id);
        }

        public bool TryGetParam(int id, out object param)
        {
            return _leases.TryGetValue(id, out param);
        }
    }
}
