using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Reitit
{
    public class HostingNavigationHelper : NavigationHelper
    {
        public NavigationHelper HostedHelper { get; set; }

        public HostingNavigationHelper(Page page) : base(page) { }

        public override bool CanGoBack()
        {
            return base.CanGoBack() && (HostedHelper == null || !HostedHelper.CanGoBack());
        }

        public override bool CanGoForward()
        {
            return base.CanGoForward() && (HostedHelper == null || !HostedHelper.CanGoForward());
        }
    }
}
