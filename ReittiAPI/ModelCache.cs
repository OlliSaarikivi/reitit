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

namespace ReittiAPI
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
            var app = Application.Current as IReittiAPIApplication;
            if (app != null)
            {
                return app.ModelCache.DoGetOrCreate(code, factory);
            }
            else
            {
                throw new Exception("Current application is not an instance of IReittiAPIApplication");
            }
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
