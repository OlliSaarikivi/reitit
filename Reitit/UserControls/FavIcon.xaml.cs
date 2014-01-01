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

namespace Reitit
{
    public partial class FavIcon : UserControl
    {
        private static Brush _white = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        public Brush IconFill;

        public FavIcon()
        {
            InitializeComponent();

            IconFill = App.Current.Resources["PhoneContrastForegroundBrush"] as Brush;
        }

        public bool Selected
        {
            set
            {
                if (value)
                {
                    RootCanvas.Background = App.Current.Resources["PhoneAccentBrush"] as Brush;
                    IconRect.Fill = _white;
                }
                else
                {
                    RootCanvas.Background = App.Current.Resources["PhoneContrastBackgroundBrush"] as Brush;
                    IconRect.Fill = IconFill;
                }
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
