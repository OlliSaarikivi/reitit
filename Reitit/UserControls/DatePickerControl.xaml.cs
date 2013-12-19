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
    public partial class DatePickerControl : DateTimePickerControl
    {
        public DatePickerControl()
        {
            InitializeComponent();
            Value = DateTime.Now;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? time = await Popup.Show(Value);
                if (time.HasValue)
                {
                    Value = time.Value;
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
