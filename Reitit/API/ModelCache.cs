using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace Reitit.API
{
    public class ModelCache
    {
        private Dictionary<string, WeakReference> _byCode = new Dictionary<string, WeakReference>();
        private Queue<object> _cached = new Queue<object>();
        private int _cacheSize;

        public ModelCache(int size)
        {
            _cacheSize = size;
        }

        public static T GetOrCreate<T>(string code, Func<T> factory)
        {
            return ((App)Application.Current).ModelCache.DoGetOrCreate(code, factory);
        }

        private T DoGetOrCreate<T>(string code, Func<T> factory)
        {
            WeakReference reference;
            T result;

            if (!_byCode.TryGetValue(code, out reference))
            {
                result = factory();
                reference = new WeakReference(result);
                _byCode[code] = reference;

                _cached.Enqueue(result);
                while (_cached.Count > _cacheSize)
                {
                    var expired = _cached.Dequeue();
                }
            }
            else
            {
                object modelInstance = reference.Target;
                if (modelInstance != null)
                {
                    result = (T)modelInstance;
                }
                else
                {
                    result = factory();
                    reference.Target = result;

                    _cached.Enqueue(result);
                    while (_cached.Count > _cacheSize)
                    {
                        var expired = _cached.Dequeue();
                    }
                }
            }

            return result;
        }
    }
}
