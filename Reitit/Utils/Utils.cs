﻿using Microsoft.Phone.Controls;
using Reitit.Resources;
using ReittiAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Windows.Devices.Geolocation;

namespace Reitit
{
    public delegate T Convert<S,T,P>(S source, P parameter, CultureInfo culture);
    public class LambdaConverter<S,T,P> : IValueConverter
    {
        private Convert<S, T, P> _convert;
        private Convert<T, S, P> _convertBack;
        public LambdaConverter(Convert<S, T, P> convert, Convert<T, S, P> convertBack = null)
        {
            _convert = convert;
            _convertBack = convertBack;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_convert == null)
            {
                throw new Exception("No forward conversion given");
            }
            return _convert((S)value, (P)parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_convertBack == null)
            {
                throw new Exception("No backward conversion given");
            }
            if (targetType != typeof(S))
            {
                throw new Exception("Incorrect target type");
            }
            return _convertBack((T)value, (P)parameter, culture);
        }
    }

    public class DatePickerConverter : LambdaConverter<DateTime, string, object>
    {
        public DatePickerConverter()
            : base((date, p, culture) =>
            {
                return date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            }) { }
    }

    public class TimePickerConverter : LambdaConverter<DateTime, string, object>
    {
        public TimePickerConverter()
            : base((date, p, culture) =>
            {
                return date.ToString("\u200E" + CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.Replace(":ss", ""));
            }) { }
    }

    public class LocationPickerConverter : LambdaConverter<IPickerLocation, string, object>
    {
        public LocationPickerConverter()
            : base ((location, p, culture) =>
            {
                return location != null ? location.Name : "";
            }) { }
    }

    public class BooleanToVisibilityConverter : LambdaConverter<bool, Visibility, object>
    {
        public BooleanToVisibilityConverter()
            : base((b, p, culture) =>
            {
                return b ? Visibility.Visible : Visibility.Collapsed;
            }) { }
    }

    public class NullToVisibilityConverter : LambdaConverter<object, Visibility, object>
    {
        public NullToVisibilityConverter()
            : base((o, p, culture) =>
            {
                return o !=  null ? Visibility.Visible : Visibility.Collapsed;
            }) { }
    }

    public class SelectedBrushConverter : LambdaConverter<bool, Brush, Brush>
    {
        public SelectedBrushConverter()
            : base((b, p, culture) =>
            {
                return b ? App.Current.AccentBrush : (p ?? App.Current.ForegroundBrush);
            }) { }
    }

    public class NullIfFalseConverter : LambdaConverter<bool, object, object>
    {
        public NullIfFalseConverter()
            : base((b, p, culture) =>
            {
                return b ? p : null;
            }) { }
    }

    public class NegationConverter : LambdaConverter<bool, bool, object>
    {
        public NegationConverter()
            : base((b, p, culture) =>
            {
                return !b;
            }) { }
    }

    public static class VisualStates
    {
        public static readonly DependencyProperty CurrentStateProperty = DependencyProperty
            .RegisterAttached(
                "CurrentState",
                typeof(string),
                typeof(VisualStates),
                new PropertyMetadata(TransitionToState));

        public static string GetCurrentState(DependencyObject obj)
        {
            return (string)obj.GetValue(CurrentStateProperty);
        }

        public static void SetCurrentState(DependencyObject obj, string value)
        {
            obj.SetValue(CurrentStateProperty, value);
        }

        static void StartOnGuiThread(Action act)
        {
            var disp = Deployment.Current.Dispatcher;
            if (disp.CheckAccess())
                act();
            else
                disp.BeginInvoke(act);
        }

        private static void TransitionToState(object sender, DependencyPropertyChangedEventArgs args)
        {
            Control elt = sender as Control;
            if (null == elt)
                throw new ArgumentException("CurrentState is only supported on Control instances");

            string newState = args.NewValue.ToString();
            StartOnGuiThread(() => VisualStateManager.GoToState(elt, newState, true));
        }
    }

    static class Utils
    {
        public static readonly double MapEpsilon = 0.000001;
        //public static readonly double MapEpsilon = 0.1;

        private static int _jiggle_index = 0;
        public static GeoCoordinate Jiggle(this GeoCoordinate coordinate)
        {
            if (_jiggle_index < 3)
            {
                coordinate.Latitude += MapEpsilon;
            }
            else if (3 < _jiggle_index && _jiggle_index < 7)
            {
                coordinate.Latitude -= MapEpsilon;
            }
            if (1 < _jiggle_index && _jiggle_index < 5)
            {
                coordinate.Longitude -= MapEpsilon;
            }
            else if (_jiggle_index == 0 || _jiggle_index > 5)
            {
                coordinate.Longitude += MapEpsilon;
            }
            _jiggle_index = (_jiggle_index + 1) % 8;
            return coordinate;
        }

        public static GeoCoordinate Copy(this GeoCoordinate coordinate)
        {
            return new GeoCoordinate
            {
                Altitude = coordinate.Altitude,
                Course = coordinate.Course,
                HorizontalAccuracy = coordinate.HorizontalAccuracy,
                Latitude = coordinate.Latitude,
                Longitude = coordinate.Longitude,
                Speed = coordinate.Speed,
                VerticalAccuracy = coordinate.VerticalAccuracy,
            };
        }

        private static Random _displace_randomizer = new Random();
        public static GeoCoordinate DisplaceFrom(this GeoCoordinate coordinate, GeoCoordinate other)
        {
            if (Math.Abs(coordinate.Longitude - other.Longitude) < MapEpsilon)
            {
                coordinate.Longitude = other.Longitude + (_displace_randomizer.Next(2) == 0 ? MapEpsilon : -MapEpsilon);
            }
            return coordinate;
        }

        public static void EnsureVisible(this ScrollViewer scrollViewer, UIElement uiElement)
        {
            scrollViewer.UpdateLayout();

            double maxScrollPos = scrollViewer.ExtentHeight - scrollViewer.ViewportHeight;
            double scrollPos = scrollViewer.VerticalOffset - scrollViewer.TransformToVisual(uiElement).Transform(new Point(0, 0)).Y;

            if (scrollPos > maxScrollPos)
            {
                scrollPos = maxScrollPos;
            }
            else if (scrollPos < 0)
            {
                scrollPos = 0;
            }

            scrollViewer.ScrollToVerticalOffset(scrollPos);
        }

        public static ReittiCoordinate ToReittiCoordinate(this Geoposition position)
        {
            return new ReittiCoordinate(position.Coordinate);
        }

        public static void KeepExpandedInView(this ListPicker picker, ScrollViewer viewer)
        {
            if (picker.ListPickerMode == ListPickerMode.Expanded)
            {
                GeneralTransform focusedVisualTransform = picker.TransformToVisual(viewer);
                Rect rectangle = focusedVisualTransform.TransformBounds(new Rect(new Point(picker.Margin.Left, picker.Margin.Top), picker.RenderSize));
                double newOffset = viewer.VerticalOffset + (rectangle.Bottom - viewer.ViewportHeight);
                if (newOffset > viewer.VerticalOffset)
                {
                    viewer.ScrollToVerticalOffset(newOffset);
                }
            }
        }

        public static bool GetIsNetworkAvailableAndWarn()
        {
            var available = NetworkInterface.GetIsNetworkAvailable();
            if (!available)
            {
                MessageBox.Show(AppResources.NetworkUnavailableText, AppResources.NetworkUnavailableTitle, MessageBoxButton.OK);
            }
            return available;
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                collection.Add(item);
            }
        }

        public static Color FlattenOn(this Color foreground, Color background)
        {
            int foreA = foreground.A;
            int backA = 255 - foreground.A;
            return Color.FromArgb(
                255,
                (byte)(((int)foreground.R * foreA + background.R * (int)backA) / 255),
                (byte)(((int)foreground.G * foreA + background.G * (int)backA) / 255),
                (byte)(((int)foreground.B * foreA + background.B * (int)backA) / 255)
                );
        }
    }
}
