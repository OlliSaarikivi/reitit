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
    public sealed partial class ColorizedIconControl : UserControl
    {
        public static readonly DependencyProperty IconBackgroundProperty = DependencyProperty.Register("IconBackground", typeof(Brush), typeof(ColorizedIconControl), new PropertyMetadata(null, IconBackgroundChanged));
        public Brush IconBackground
        {
            get { return (Brush)this.GetValue(IconBackgroundProperty); }
            set { this.SetValue(IconBackgroundProperty, value); }
        }
        private static void IconBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as ColorizedIconControl;
            if (me != null)
            {
                me.BacktroundRectangle.Fill = e.NewValue as Brush;
            }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(ColorizedIconControl), new PropertyMetadata(null, IconChanged));
        public ImageSource Icon
        {
            get { return (ImageSource)this.GetValue(IconProperty); }
            set { this.SetValue(IconProperty, value); }
        }
        private static void IconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var me = (ColorizedIconControl)d;
            //me.Rect.OpacityMask = new ImageBrush
            //{
            //    ImageSource = e.NewValue as ImageSource,
            //    Stretch = Stretch.None,
            //    AlignmentX = AlignmentX.Center,
            //    AlignmentY = AlignmentY.Center,
            //};
        }

        public ColorizedIconControl()
        {
            this.InitializeComponent();
        }
    }
}
