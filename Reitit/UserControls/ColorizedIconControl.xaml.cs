using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace Reitit
{
    public partial class ColorizedIconControl : UserControl
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
            var me = (ColorizedIconControl)d;
            me.Rect.OpacityMask = new ImageBrush
            {
                ImageSource = e.NewValue as ImageSource,
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center,
            };
        }

        public ColorizedIconControl()
        {
            InitializeComponent();
        }

    }
}
