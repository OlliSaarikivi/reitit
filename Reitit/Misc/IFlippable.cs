using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Reitit
{
    public enum FlipPreference
    {
        East, West
    }

    interface IFlippable
    {
        void SetFlip(FlipPreference preference);
    }
}
