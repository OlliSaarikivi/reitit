using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    public class Setting<T>
    {
        private string _key;
        private Func<T> _defaultValueFactory;

        public T Value
        {
            get
            {
                T value;
                if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(_key, out value))
                {
                    value = _defaultValueFactory();
                }
                return value;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings[_key] = value;
            }
        }

        public Setting(string key, Func<T> defaultValueFactory)
        {
            _key = key;
            _defaultValueFactory = defaultValueFactory;
        }

        public Setting(string key, T defaultValue)
            : this(key, () => defaultValue)
        {
        }
    }
}
