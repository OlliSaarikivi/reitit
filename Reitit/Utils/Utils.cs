using ReittiAPI;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
                return date.ToString(culture.DateTimeFormat.ShortDatePattern);
            }) { }
    }

    public class TimePickerConverter : LambdaConverter<DateTime, string, object>
    {
        public TimePickerConverter()
            : base((date, p, culture) =>
            {
                return date.ToString("\u200E" + culture.DateTimeFormat.LongTimePattern.Replace(":ss", ""));
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

    static class Utils
    {
        //public static readonly double MapEpsilon = 0.000001;
        public static readonly double MapEpsilon = 0.1;

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
    }
}
