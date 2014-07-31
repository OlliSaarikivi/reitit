using Reitit.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Reitit
{
    public delegate T Convert<S, T, P>(S source, P parameter, string language);
    public class LambdaConverter<S, T, P> : IValueConverter
    {
        private Convert<S, T, P> _convert;
        private Convert<T, S, P> _convertBack;
        public LambdaConverter(Convert<S, T, P> convert, Convert<T, S, P> convertBack = null)
        {
            _convert = convert;
            _convertBack = convertBack;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (_convert == null)
            {
                throw new Exception("No forward conversion given");
            }
            return _convert((S)value, (P)parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (_convertBack == null)
            {
                throw new Exception("No backward conversion given");
            }
            if (targetType != typeof(S))
            {
                throw new Exception("Incorrect target type");
            }
            return _convertBack((T)value, (P)parameter, language);
        }
    }

    public class ShortTimeConverter : LambdaConverter<DateTime, string, object>
    {
        public ShortTimeConverter()
            : base((date, p, language) =>
            {
                return Utils.GetTimeFormatter().Format(date);
            }) { }
    }

    public class LocationPickerConverter : LambdaConverter<IPickerLocation, string, object>
    {
        public LocationPickerConverter()
            : base((location, p, language) =>
            {
                return location != null ? location.Name : "";
            }) { }
    }

    public class VisibilityConverter : LambdaConverter<bool, Visibility, object>
    {
        public VisibilityConverter()
            : base((b, p, language) =>
            {
                return b ? Visibility.Visible : Visibility.Collapsed;
            }) { }
    }

    public class NegVisibilityConverter : LambdaConverter<bool, Visibility, object>
    {
        public NegVisibilityConverter()
            : base((b, p, language) =>
            {
                return b ? Visibility.Collapsed : Visibility.Visible;
            }) { }
    }

    public class NullToVisibilityConverter : LambdaConverter<object, Visibility, object>
    {
        public NullToVisibilityConverter()
            : base((o, p, language) =>
            {
                return o != null ? Visibility.Visible : Visibility.Collapsed;
            }) { }
    }

    public class NegNullToVisibilityConverter : LambdaConverter<object, Visibility, object>
    {
        public NegNullToVisibilityConverter()
            : base((o, p, language) =>
            {
                return o == null ? Visibility.Visible : Visibility.Collapsed;
            }) { }
    }

    public class TransparentIfNullConverter : LambdaConverter<object, double, object>
    {
        public TransparentIfNullConverter()
            : base((o, p, language) =>
            {
                return o != null ? 1 : 0;
            }) { }
    }

    public class OpaqueIfNullConverter : LambdaConverter<object, double, object>
    {
        public OpaqueIfNullConverter()
            : base((o, p, language) =>
            {
                return o == null ? 1 : 0;
            }) { }
    }

    public class TransparentIfTrueConverter : LambdaConverter<bool, double, object>
    {
        public TransparentIfTrueConverter()
            : base((o, p, language) =>
            {
                return o ? 0 : 1;
            }) { }
    }

    public class OpaqueIfTrueConverter : LambdaConverter<bool, double, object>
    {
        public OpaqueIfTrueConverter()
            : base((o, p, language) =>
            {
                return o ? 1 : 0;
            }) { }
    }

    public class SelectedBrushConverter : LambdaConverter<bool, Brush, Brush>
    {
        public SelectedBrushConverter()
            : base((b, p, language) =>
            {
                return b ? (Brush)App.Current.Resources["PhoneAccentBrush"] : (p ?? (Brush)App.Current.Resources["PhoneForegroundBrush"]);
            }) { }
    }

    public class ForegroundBrushConverter : LambdaConverter<bool, Brush, Brush>
    {
        public ForegroundBrushConverter()
            : base((b, p, language) =>
            {
                return b ? (Brush)App.Current.Resources["PhoneForegroundBrush"] : (p ?? (Brush)App.Current.Resources["PhoneBackgroundBrush"]);
            }) { }
    }

    public class NullIfFalseConverter : LambdaConverter<bool, object, object>
    {
        public NullIfFalseConverter()
            : base((b, p, language) =>
            {
                return b ? p : null;
            }) { }
    }

    public class NegationConverter : LambdaConverter<bool, bool, object>
    {
        public NegationConverter()
            : base((b, p, language) =>
            {
                return !b;
            }) { }
    }

    public class NotNullConverter : LambdaConverter<object, bool, object>
    {
        public NotNullConverter()
            : base((o, p, language) =>
            {
                return o != null;
            }) { }
    }

    public class IsNullConverter : LambdaConverter<object, bool, object>
    {
        public IsNullConverter()
            : base((o, p, language) =>
            {
                return o == null;
            }) { }
    }

    public class DefaultIfNullConverter : LambdaConverter<object, object, object>
    {
        public DefaultIfNullConverter()
            : base((o, p, language) =>
            {
                return o != null ? o : p;
            }) { }
    }

    public class EqualConverter : LambdaConverter<object, object, object>
    {
        public EqualConverter()
            : base((o, p, language) =>
            {
                return o == p;
            }) { }
    }

    public class ColorToOpacityConverter : LambdaConverter<Color, double, object>
    {
        public ColorToOpacityConverter()
            : base((c, p, language) =>
            {
                return (double)c.A / byte.MaxValue;
            }) { }
    }

    public class ToUpperCaseConverter : LambdaConverter<string, string, object>
    {
        public ToUpperCaseConverter()
            : base((s, p, language) =>
            {
                return s.ToUpper();
            }) { }
    }

    public class PickerLocationToCoordinateConverter : LambdaConverter<IPickerLocation, ReittiCoordinate, object>
    {
        public PickerLocationToCoordinateConverter()
            : base((s, p, language) =>
            {
                return s != null ? s.GetCoordinatesSynchronouslyOrNone() : null;
            }) { }
    }

    public class PickerLocationToVisibilityConverter : LambdaConverter<IPickerLocation, Visibility, object>
    {
        public PickerLocationToVisibilityConverter()
            : base((s, p, language) =>
            {
                return s != null ? (s.GetCoordinatesSynchronouslyOrNone() != null ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
            }) { }
    }

    public class ReittiCoordinateToGeopointConverter : LambdaConverter<ReittiCoordinate, Geopoint, object>
    {
        public ReittiCoordinateToGeopointConverter()
            : base(
            (s, p, language) =>
            {
                return s;
            },
            (s, p, language) =>
            {
                return (ReittiCoordinate)s;
            })
        { }
    }

    public class HiddenIfEmptyConverter : LambdaConverter<IList, Visibility, object>
    {
        public HiddenIfEmptyConverter()
            : base((s, p, language) =>
            {
                return s.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }) { }
    }

    public class VisibleIfEmptyConverter : LambdaConverter<IList, Visibility, object>
    {
        public VisibleIfEmptyConverter()
            : base((s, p, language) =>
            {
                return s.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }) { }
    }
}
