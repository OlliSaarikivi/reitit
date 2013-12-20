using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Reitit
{
    public abstract class DateTimePickerControl : PickerControl<DateTime, DateTime?> { }
    public abstract class PickerControl<T,U> : UserControl
    {
        public static DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(T), typeof(PickerControl<T,U>), new PropertyMetadata(default(T)));
        public T Value
        {
            get { return (T)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty PopupProperty = DependencyProperty.Register("Popup", typeof(PickerPopup<U>), typeof(PickerControl<T,U>), new PropertyMetadata(null));
        public PickerPopup<U> Popup
        {
            get { return (PickerPopup<U>)this.GetValue(PopupProperty); }
            set { this.SetValue(PopupProperty, value); }
        }
    }
}
