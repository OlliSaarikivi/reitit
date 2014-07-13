using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    public interface IIndicatorManager
    {
        Task Init();
        Task PushStatus(TrayStatus status);
        Task RemoveStatus(TrayStatus status);
        Task UpdateIndicator();
    }
}
