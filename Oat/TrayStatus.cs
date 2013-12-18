using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Oat
{
    public class TrayStatus : ProgressIndicator, IDisposable
    {
        public TrayStatus(string text = "", bool isIndeterminate = true, double value = 0)
        {
            Text = text;
            IsIndeterminate = isIndeterminate;
            Value = value;
            IsVisible = true;

            // Next this status will be registered in the manager. All data should be initialized at this point.
            var oatApplication = Application.Current as IOatApplication;
            if (oatApplication != null)
            {
                oatApplication.IndicatorManager.PushIndicator(this);
            }
            else
            {
                throw new Exception("The current application is not an instance of IOatApplication");
            }
        }

        public void Dispose()
        {
            var oatApplication = Application.Current as IOatApplication;
            if (oatApplication != null)
            {
                oatApplication.IndicatorManager.RemoveIndicator(this);
            }
            else
            {
                throw new Exception("The current application is not an instance of IOatApplication");
            }
        }
    }
}
