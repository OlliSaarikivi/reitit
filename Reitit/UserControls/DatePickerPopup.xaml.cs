using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls.Primitives;
using System.Globalization;
using GalaSoft.MvvmLight.Command;

namespace Reitit
{
    public partial class DatePickerPopup : DateTimePickerPopup
    {
        public DatePickerPopup()
        {
            InitializeComponent();

            DoneButton.Command = new RelayCommand(() =>
            {
                Done(((DateTimeWrapper)_primarySelectorPart.DataSource.SelectedItem).DateTime);
            });
            CancelButton.Command = new RelayCommand(() =>
            {
                Done(null);
            });

            // Hook up the data sources
            PrimarySelector.DataSource = new YearDataSource();
            SecondarySelector.DataSource = new MonthDataSource();
            TertiarySelector.DataSource = new DayDataSource();

            InitializeDateTimePickerPage(PrimarySelector, SecondarySelector, TertiarySelector);
        }

        protected override IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern()
        {
            string pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToUpperInvariant();

            return GetSelectorsOrderedByCulturePattern(
                pattern,
                new char[] { 'Y', 'M', 'D' },
                new LoopingSelector[] { PrimarySelector, SecondarySelector, TertiarySelector });
        }
    }
}
