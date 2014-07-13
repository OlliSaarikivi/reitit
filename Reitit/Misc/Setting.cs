using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Reitit
{
    public enum SettingScope
    {
        Local, Roaming
    }

    public class Setting<T>
    {
        private SettingScope _scope;
        private string _key;
        private Func<T> _defaultValueFactory;

        public T Value
        {
            get
            {
                object value;
                var container = ContainerForScope();
                if (!container.Values.TryGetValue(_key, out value))
                {
                    return _defaultValueFactory();
                }
                if (value is T)
                {
                    return (T)value;
                }
                else
                {
                    T typedValue = _defaultValueFactory();
                    container.Values[_key] = typedValue;
                    return typedValue;
                }
            }
            set
            {
                var container = ContainerForScope();
                container.Values[_key] = value;
            }
        }

        private ApplicationDataContainer ContainerForScope()
        {
            switch (_scope)
            {
                case SettingScope.Local:
                    return ApplicationData.Current.LocalSettings;
                case SettingScope.Roaming:
                    return ApplicationData.Current.RoamingSettings;
                default:
                    throw new Exception("Uknown setting scope");
            }
        }

        public Setting(string key, Func<T> defaultValueFactory, SettingScope scope = SettingScope.Local)
        {
            _key = key;
            _defaultValueFactory = defaultValueFactory;
            _scope = scope;
        }

        public Setting(string key, T defaultValue = default(T), SettingScope scope = SettingScope.Local)
            : this(key, () => defaultValue, scope)
        {
        }
    }
}
