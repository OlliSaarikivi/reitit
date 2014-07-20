using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace Reitit
{
    public class StackIndicatorManager
    {
        List<TrayStatus> _indicators = new List<TrayStatus>();

        public async Task Init()
        {
            await UpdateIndicator();
        }

        public async Task PushStatus(TrayStatus status)
        {
            _indicators.Add(status);

            await UpdateIndicator();
        }

        public async Task RemoveStatus(TrayStatus indicator)
        {
            if (_indicators.Remove(indicator))
            {
                await UpdateIndicator();
            }
        }

        public async Task SurfaceStatus(TrayStatus indicator)
        {
            if (_indicators.Count != 0)
            {
                if (_indicators.LastElement() != indicator)
                {
                    _indicators.Remove(indicator);
                    _indicators.Add(indicator);
                    await UpdateIndicator();
                }
            }
        }

        public async Task UpdateIndicator()
        {
            if (_indicators.Count == 0)
            {
                var bar = StatusBar.GetForCurrentView();
                await bar.ProgressIndicator.HideAsync();
            }
            else
            {
                var status = _indicators[_indicators.Count - 1];

                var bar = StatusBar.GetForCurrentView();
                bar.ProgressIndicator.ProgressValue = status.Value;
                bar.ProgressIndicator.Text = status.Text;
                await bar.ProgressIndicator.ShowAsync();
            }
        }
    }
}
