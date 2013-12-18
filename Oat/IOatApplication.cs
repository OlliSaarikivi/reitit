using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oat
{
    public interface IOatApplication
    {
        IIndicatorManager IndicatorManager { get; }
    }
}
