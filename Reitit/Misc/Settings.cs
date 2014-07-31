using Enough.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;

namespace Reitit
{
    [DataContract]
    public class Settings : ExtendedObservableObject
    {
        [DataMember]
        public int ParamCacheNextId = 1;
        [DataMember]
        public List<string> PickerSearchHistory = new List<string>();
        [DataMember]
        public List<string> StopSearchHistory = new List<string>();
        [DataMember]
        public List<string> LineSearchHistory = new List<string>();
        [DataMember]
        public ObservableCollection<RecentLocation> RecentLocations = new ObservableCollection<RecentLocation>();
        [DataMember]
        public FavoritesManagerSettings Favorites = new FavoritesManagerSettings();
        [DataMember]
        public int CustomIconFreeIndex = -1;
        [DataMember]
        public int DefaultTransferMargin = 3;
        [DataMember]
        public int DefaultRouteTypeIndex = 0;
        [DataMember]
        public int DefaultSpeedIndex = 1;

        public static async Task Save(Settings settings)
        {
            await StorageHelper.SaveObjectAsync(settings, "Settings");
        }

        public static async Task<Settings> Load()
        {
            Settings settings = await StorageHelper.TryLoadObjectAsync<Settings>("Settings");
            if (settings == null)
            {
                settings = new Settings();
            }
            return settings;
        }
    }

    //public class Setting<T>
    //{
    //    private string _key;
    //    private Func<T> _defaultValueFactory;

    //    public Setting(string key, Func<T> defaultValueFactory)
    //    {
    //        _key = key;
    //        _defaultValueFactory = defaultValueFactory;
    //    }

    //    public Setting(string key, T defaultValue = default(T))
    //        : this(key, () => defaultValue)
    //    {
    //    }

    //    private string FileName() { return "Setting_" + _key; }

    //    public T Value
    //    {
    //        get
    //        {
    //            object obj;
    //            if (!Setting.SettingsStore.TryGetValue(_key, out obj))
    //            {
    //                T typed = _defaultValueFactory();
    //                Setting.SettingsStore[_key] = typed;
    //                return typed;
    //            } else {
    //                return (T)obj;
    //            }
    //        }
    //        set
    //        {
    //            Setting.SettingsStore[_key] = value;
    //        }
    //    }
    //}
}
