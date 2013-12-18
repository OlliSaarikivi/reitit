using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oat
{
    public interface IIndicatorManager
    {
        void Init();
        void PushIndicator(ProgressIndicator indicator);
        void RemoveIndicator(ProgressIndicator indicator);
    }
}
