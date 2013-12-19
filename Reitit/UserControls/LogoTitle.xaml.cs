using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Reitit
{
    public partial class LogoTitle : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(LogoTitle), new PropertyMetadata(null, TextChanged));
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); } 
        }
        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as LogoTitle;
            if (me != null)
            {
                me.TextBlock.Text = e.NewValue as string;
            }
        }

        public LogoTitle()
        {
            InitializeComponent();
        }
    }
}
