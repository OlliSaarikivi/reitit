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
        public ColorizedIconControl()
        {
            InitializeComponent();
        }

        public Brush IconBackground
        {
            set
            {
                BacktroundRectangle.Fill = value;
            }
        }

        public ImageSource Icon
        {
            set
            {
                Mask.ImageSource = value;
            }
        }
    }
}
