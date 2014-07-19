using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Reitit
{
    [ContentProperty(Name="Template")]
    class ReititMapItem
    {
        public DataTemplate Template { get; set; }

        public UIElement Element
        {
            get
            {
                if (_element == null)
                {
                    _element = Template.LoadContent() as UIElement;
                }
                return _element;
            }
        }
        private UIElement _element;
    }
}
