using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Reitit
{
    public sealed partial class LabelPushpin : UserControl, IFlippable
    {
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(LabelPushpin), new PropertyMetadata(null));

        int IFlippable.Importance
        {
            get
            {
                return Label == null || Label.Length == 0 ? 1 : 1000000;
            }
        }

        public LabelPushpin()
        {
            this.InitializeComponent();
        }

        public void SetFlip(FlipPreference preference)
        {
            FlipTransform.ScaleX = preference == FlipPreference.West ? -1 : 1;
        }
    }
}
