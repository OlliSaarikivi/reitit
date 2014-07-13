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
            await ((App)Application.Current).IndicatorManager.UpdateIndicator();
        }
        public double? Value { get; private set; }
        public async Task SetValue(double? value)
        {
            Value = value;
            await ((App)Application.Current).IndicatorManager.UpdateIndicator();
        }

        public TrayStatus(string text = "", double? value = null)
        {
            Text = text;
            Value = value;
        }

        public async Task Push()
        {
            await ((App)Application.Current).IndicatorManager.PushStatus(this);
        }

        public async Task Remove()
        {
            await ((App)Application.Current).IndicatorManager.RemoveStatus(this);
        }
    }
}
