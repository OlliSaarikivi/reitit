using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Reitit
{
    public class ParameterCache
    {
        private Dictionary<int, WeakReference> _paramStore = new Dictionary<int, WeakReference>();
        private int _nextId = 0;

        private int GetParamId()
        {
            return _nextId++;
        }

        public int AddParam(object param)
        {
            int id = GetParamId();
            _paramStore.Add(id, new WeakReference(param));

            return id;
        }

        public object RemoveParam(IDictionary<string, string> queryString, string key)
        {
            object param;
            if (TryRemoveParam(queryString, key, out param))
            {
                return param;
            }
            throw new Exception("No parameter found for key " + key);
        }

        public bool TryRemoveParam<T>(IDictionary<string, string> queryString, string key, out T param) where T : class
        {
            string idString;
            int id;
            if (queryString.TryGetValue(key, out idString) && int.TryParse(idString, out id))
            {
                return TryRemoveParam(id, out param);
            }
            param = null;
            return false;
        }

        public bool TryRemoveParam<T>(int id, out T param) where T : class
        {
            WeakReference reference;
            if (_paramStore.TryGetValue(id, out reference))
            {
                _paramStore.Remove(id);
                object paramObj = reference.Target;
                if (paramObj == null)
                {
                    param = null;
                    return false;
                }
                else
                {
                    param = (T)paramObj;
                    return true;
                }
            }
            param = null;
            return false;
        }
    }
}
