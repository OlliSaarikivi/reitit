using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Reitit
{
    public sealed partial class FocusAllButton : UserControl
    {
        public bool Shown
        {
            get { return (bool)GetValue(ShownProperty); }
            set { SetValue(ShownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Shown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShownProperty =
            DependencyProperty.Register("Shown", typeof(bool), typeof(FocusAllButton), new PropertyMetadata(false, ShownChanged));

        private static void ShownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FocusAllButton;
            if (control != null)
            {
                VisualStateManager.GoToState(control, (bool)e.NewValue ? "Visible" : "Hidden", true);
            }
        }

        public FocusAllButton()
        {
            this.InitializeComponent();
            VisualStateManager.GoToState(this, "Hidden", false);
        }

        public event Action FocusAll;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var handler = FocusAll;
            if (handler != null)
            {
                handler();
            }
        }
    }
}
