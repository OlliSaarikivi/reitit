using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Xaml;

namespace Reitit
{
    public class TrayStatus
    {
        public string Text { get; private set; }
        public async Task SetText(string text)
        {
            Text = text;
            await App.Current.IndicatorManager.UpdateIndicator();
        }
        public double? Value { get; private set; }
        public async Task SetValue(double? value)
        {
            Value = value;
            await App.Current.IndicatorManager.UpdateIndicator();
        }

        public bool Active { get; private set; }

        public TrayStatus(string text = "", double? value = null)
        {
            Text = text;
            Value = value;
            Active = false;
        }

        public async Task Push()
        {
            if (!Active)
            {
                Active = true;
                await App.Current.IndicatorManager.PushStatus(this);
            }
            else
            {
                await App.Current.IndicatorManager.SurfaceStatus(this);
            }
        }

        public async Task Remove()
        {
            if (Active)
            {
                await App.Current.IndicatorManager.RemoveStatus(this);
                Active = false;
            }
        }
    }
}
