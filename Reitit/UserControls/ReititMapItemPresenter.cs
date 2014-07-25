using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Reitit
{
    [ContentProperty(Name = "Template")]
    class ReititMapItemPresenter
    {
        public DataTemplate Template { get; set; }

        private object _dataContext;

        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                _dataContext = value;
                if (CurrentElement != null)
                {
                    CurrentElement.DataContext = _dataContext;
                }
            }
        }


        public FrameworkElement CurrentElement { get; set; }

        public FrameworkElement GenerateElement()
        {
            CurrentElement = Template.LoadContent() as FrameworkElement;
            CurrentElement.DataContext = DataContext;
            return CurrentElement;
        }
    }
}
