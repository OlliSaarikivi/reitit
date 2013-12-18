using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oat
{
    public class StackIndicatorManager : IIndicatorManager
    {
        List<ProgressIndicator> _indicators = new List<ProgressIndicator>();

        public void Init()
        {
            UpdateIndicator();
        }

        public void PushIndicator(ProgressIndicator indicator)
        {
            _indicators.Add(indicator);
            SystemTray.ProgressIndicator = indicator;
        }

        public void RemoveIndicator(ProgressIndicator indicator)
        {
            if (_indicators.Remove(indicator))
            {
                UpdateIndicator();
            }
        }

        private void UpdateIndicator()
        {
            if (_indicators.Count == 0)
            {
                SystemTray.ProgressIndicator = null;
            }
            else
            {
                ProgressIndicator topIndicator = _indicators[_indicators.Count - 1];
                SystemTray.ProgressIndicator = topIndicator;
            }
        }
    }
}
