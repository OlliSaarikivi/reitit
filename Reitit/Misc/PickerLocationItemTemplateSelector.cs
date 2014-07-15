using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Reitit
{
    public class PickerLocationListTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ReittiLocationBase)
            {
                return (DataTemplate)App.Current.Resources["ReittiLocationListTemplate"];
            }
            if (item is FavoritePickerLocation)
            {
                return (DataTemplate)App.Current.Resources["FavoriteLocationListTemplate"];
            }
            if (item is RecentPickerLocation)
            {
                return (DataTemplate)App.Current.Resources["RecentLocationListTemplate"];
            }
            return null;
        }
    }
}
