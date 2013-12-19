using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Controls.Primitives;
using System.Globalization;

namespace Reitit
{
    public partial class TimePickerPopup : DateTimePickerPopup
    {
        public TimePickerPopup()
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
            PrimarySelector.DataSource = DateTimeWrapper.CurrentCultureUsesTwentyFourHourClock() ?
                 (DataSource)(new TwentyFourHourDataSource()) :
                 (DataSource)(new TwelveHourDataSource());
            SecondarySelector.DataSource = new MinuteDataSource();
            TertiarySelector.DataSource = new AmPmDataSource();

            InitializeDateTimePickerPage(PrimarySelector, SecondarySelector, TertiarySelector);
        }

        protected override IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern()
        {
            string pattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.ToUpperInvariant();

            return GetSelectorsOrderedByCulturePattern(
                pattern,
                new char[] { 'H', 'M', 'T' },
                new LoopingSelector[] { PrimarySelector, SecondarySelector, TertiarySelector });
        }
    }
}
