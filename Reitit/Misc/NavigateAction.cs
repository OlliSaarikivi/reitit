using Microsoft.Xaml.Interactions.Core;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Reitit
{
    class NavigateAction : DependencyObject, IAction
    {
        public string Page
        {
            get { return (string)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        public static readonly DependencyProperty PageProperty =
            DependencyProperty.Register("Page", typeof(string), typeof(NavigateAction), new PropertyMetadata(null));

        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Parameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(NavigateAction), new PropertyMetadata(null));

        public object Execute(object sender, object parameter)
        {
            Utils.Navigate(Type.GetType("Reitit." + Page), Parameter);
            return null;
        }
    }
}
