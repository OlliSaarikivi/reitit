using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Reflection;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Messaging;
using Windows.UI.Xaml.Controls.Primitives;
using GalaSoft.MvvmLight.Command;
using Windows.Devices.Geolocation;
using Reitit.API;
using Windows.Storage.Streams;
using Windows.Foundation;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.Globalization.DateTimeFormatting;
using Windows.System.UserProfile;
using Windows.Globalization;

namespace Reitit
{
    public static partial class Utils
    {
        public static readonly string DefaultFavIcon = "FavPoi";

        public static readonly double MapEpsilon = 0.0000001;
        public static readonly double MinZoomedLatitudeDiff = 0.0025;
        public static readonly double MinZoomedLongitudeDiff = 0.00666;
        public static readonly double PushpinAvoidDiffY = 0.025;
        public static readonly double PushpinAvoidDiffX = 0.04;

        public static readonly Color FromColor = Color.FromArgb(255, 0, 160, 0);
        public static readonly Color ToColor = Color.FromArgb(255, 160, 15, 0);

        public static readonly ReittiCoordinate HelsinkiCoordinate = new ReittiCoordinate(60.1708, 24.9375);
        public static readonly ReittiCoordinate DefaultViewCoordinate = new ReittiCoordinate(60.188057413324714, 24.890878032892942);
        public static readonly double DefaultViewZoom = 10.00100040435791;
        public static readonly double ShowLocationZoom = 15;

        public static async Task OnCoreDispatcher(DispatchedHandler handler, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(priority, handler);
        }

        public static async Task UsingStatus(TrayStatus status, Func<Task> action)
        {
            await status.Push();
            await action();
            await status.Remove();
        }
        public static async Task UsingStatus(string text, Func<Task> action)
        {
            await UsingStatus(new TrayStatus(text), action);
        }

        public static void AddRange<T>(this Collection<T> collection, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                collection.Add(item);
            }
        }

        public static T LastElement<T>(this IList<T> list)
        {
            return list[list.Count - 1];
        }

        private static ResourceLoader _loader = new ResourceLoader();
        public static string GetString(string key)
        {
            return _loader.GetString(key);
        }

        public static string FormatLineCode(string shortName, int? typeId)
        {
            string lineText = shortName;
            if (typeId != null)
            {
                if (typeId.Value == 1 || typeId.Value == 21 || typeId.Value == 22)
                {
                    lineText += ", Helsinki";
                }
                else if (typeId.Value == 3 || typeId.Value == 23)
                {
                    lineText += ", Espoo";
                }
                else if (typeId.Value == 4 || typeId.Value == 24)
                {
                    lineText += ", Vantaa";
                }
                else if (36 == typeId.Value)
                {
                    lineText += ", Kirkkonummi";
                }
                else if (39 == typeId.Value)
                {
                    lineText += ", Kerava";
                }
            }
            return lineText;
        }

        private static string[] _defaultLanguagePreference = { "en", "fi", "sv", "slangi" };

        public static string GetPreferredName(Dictionary<string, string> namesByLanguage)
        {
            string preferredName;

            string currentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            if (namesByLanguage.TryGetValue(currentLanguage, out preferredName))
            {
                return preferredName;
            }

            foreach (string language in _defaultLanguagePreference)
            {
                if (namesByLanguage.TryGetValue(language, out preferredName))
                {
                    return preferredName;
                }
            }

            return null;
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

        public static string FormatDistance(double distance)
        {
            if (distance < 1000)
            {
                return distance.ToString("0") + "\u00A0m";
            }
            else
            {
                return (distance / 1000).ToString("0.0") + "\u00A0km";
            }
        }

        public static string FormatDistanceConserveSpace(double distance)
        {
            if (distance < 1000)
            {
                return distance.ToString("0") + "\u2006m";
            }
            else
            {
                return (distance / 1000).ToString("0.0") + "\u2006km";
            }
        }

        public static string FormatTimeSpan(TimeSpan time)
        {
            if (time.TotalHours < 1)
            {
                return time.ToString("m'\u00A0min'");
            }
            else
            {
                if (time.Minutes == 0)
                    return time.ToString("h'\u00A0h'");
                else
                    return time.ToString("h'\u00A0h 'm'\u00A0min'");
            }
        }

        public static bool GetIsNetworkAvailableAndWarn()
        {
            var available = NetworkInterface.GetIsNetworkAvailable();
            if (!available)
            {
                Utils.ShowMessageDialog(Utils.GetString("NetworkUnavailableText"), Utils.GetString("NetworkUnavailableTitle"));
            }
            return available;
        }

        public static bool CurrentCultureUsesTwentyFourHourClock()
        {
            return !CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.Contains("t");
        }

        public static string ToShortTimeString(DateTime time)
        {
            return time.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
        }

        public static void Navigate(Type page, object parameter = null)
        {
            var frame = ((Frame)Window.Current.Content);
            if (page.GetTypeInfo().IsSubclassOf(typeof(MapContentPage)))
            {
                if (frame.CurrentSourcePageType == typeof(MapPage))
                {
                    Messenger.Default.Send(new MapPageNavigateMessage(page, App.Current.ParamCache.AddParam(parameter)));
                }
                else
                {
                    frame.Navigate(typeof(MapPage), App.Current.ParamCache.AddParam(new MapPageNavigateMessage(page, App.Current.ParamCache.AddParam(parameter))));
                }
            }
            else
            {
                frame.Navigate(page, App.Current.ParamCache.AddParam(parameter));
            }
        }

        public static void ShowMessageFlyout(string message, string title)
        {
            var flyout = new Flyout
            {
                Placement = FlyoutPlacementMode.Top,
                FlyoutPresenterStyle = (Style)Application.Current.Resources["NoScrollFlyoutPresenterStyle"],
                Content = new MessageFlyoutContent
                {
                    Title = title,
                    Message = message,
                    VerticalAlignment = VerticalAlignment.Stretch,
                },
            };
            flyout.ShowAt((Frame)Window.Current.Content);
        }

        public static async Task ShowMessageDialog(string message, string title)
        {
            var dialog = new MessageDialog(message, title);
            await dialog.ShowAsync();
        }

        private static Random _displace_randomizer = new Random();
        public static Geopoint Jiggle(this Geopoint point)
        {
            var newLongitude = point.Position.Longitude + (_displace_randomizer.Next(2) == 0 ? MapEpsilon : -MapEpsilon);
            var position = new BasicGeoposition
            {
                Latitude = point.Position.Latitude,
                Longitude = point.Position.Longitude,
                Altitude = point.Position.Altitude,
            };
            return new Geopoint(position, point.AltitudeReferenceSystem, point.SpatialReferenceId);
        }

        public static int UpperBound<T>(this IList<T> list, T key, IComparer<T> comparer)
        {
            int begin = 0;
            int end = list.Count;
            while (begin < end)
            {
                int middle = (begin + end) / 2;
                T element = list[middle];
                if (comparer.Compare(element, key) >= 0)
                {
                    end = middle;
                }
                else
                {
                    begin = middle + 1;
                }
            }
            return begin;
        }

        public static ReittiCoordinate GetCoordinatesSynchronouslyOrNone(this IPickerLocation location)
        {
            if (!(location is MeLocation))
            {
                var task = location.GetCoordinates();
                return task.IsCompleted ? task.Result : null;
            }
            return null;
        }
        public static List<T> GetChildrenOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            List<T> results = new List<T>();

            if (depObj == null) return results;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = child as T;
                if (result != null)
                {
                    results.Add(result);
                }
                results.AddRange(GetChildrenOfType<T>(child));
            }
            return results;
        }

        public static Brush BrushForType(string type)
        {
            string key = "BusBrush";
            if (type == "walk" || type == "wait")
            {
                key = "WalkBrush";
            }
            else if (type == "tram" || type == "2")
            {
                key = "TramBrush";
            }
            else if (type == "metro" || type == "6")
            {
                key = "MetroBrush";
            }
            else if (type == "ferry" || type == "7")
            {
                key = "FerryBrush";
            }
            else if (type == "train" || type == "12")
            {
                key = "TrainBrush";
            }
            return (Brush)App.Current.Resources[key];
        }

        public static Color ColorForType(string type)
        {
            return (BrushForType(type) as SolidColorBrush).Color;
        }

        public static RandomAccessStreamReference MapIconImageForType(string type)
        {
            string elementIconName = "BusStop";
            if (type == "walk" || type == "wait")
            {
                elementIconName = "DefaultStop";
            }
            else if (type == "tram" || type == "2")
            {
                elementIconName = "TramStop";
            }
            else if (type == "metro" || type == "6")
            {
                elementIconName = "MetroStop";
            }
            else if (type == "ferry" || type == "7")
            {
                elementIconName = "FerryStop";
            }
            else if (type == "train" || type == "12")
            {
                elementIconName = "TrainStop";
            }
            return RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/MapElements/" + elementIconName + ".png"));
        }

        public static string GetDescriptiveShortLineName(Line line)
        {
            if (line.Code.StartsWith("13") && line.LineEnd != null)
            {
                return line.ShortName + Utils.GetString("MetroDestinationSeparator") + line.LineEnd;
            }
            else
            {
                return line.ShortName;
            }
        }

        public static ReittiBoundingBox GetBoundingBox(IEnumerable<ReittiCoordinate> coordinates)
        {
            bool atLeastOne = false;
            double maxLatitude = double.MinValue, minLatitude = double.MaxValue, maxLongitude = double.MinValue, minLongitude = double.MaxValue;
            foreach (var c in coordinates)
            {
                if (c != null)
                {
                    atLeastOne = true;
                    if (c.Latitude > maxLatitude) maxLatitude = c.Latitude;
                    if (c.Latitude < minLatitude) minLatitude = c.Latitude;
                    if (c.Longitude > maxLongitude) maxLongitude = c.Longitude;
                    if (c.Longitude < minLongitude) minLongitude = c.Longitude;
                }
            }
            return atLeastOne ? new ReittiBoundingBox(minLongitude, maxLatitude, maxLongitude, minLatitude) : null;
        }

        public static double SquaredDistanceTo(this ReittiCoordinate coordinate, ReittiCoordinate other)
        {
            var latDiff = coordinate.Latitude - other.Latitude;
            var longDiff = coordinate.Longitude - other.Longitude;
            return latDiff * latDiff + longDiff * longDiff;
        }

        public static async Task ShowOperationFailedError()
        {
            await ShowMessageDialog(Utils.GetString("OperationFailedErrorMessage"), Utils.GetString("OperationFailedErrorTitle"));
        }

        public static void D(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        private static string SelectFormattingLanguage()
        {
            var languages = new List<string>(ApplicationLanguages.Languages);
            if (languages.Contains("fi") || languages.Contains("fi-FI"))
            {
                return "fi";
            }
            else
            {
                return ApplicationLanguages.Languages[0];
            }
        }

        public static DateTimeFormatter GetDateTimeFormatter()
        {
            string formattingLanguage = SelectFormattingLanguage();
            var dateFormatter = new DateTimeFormatter("shortdate", new[] { formattingLanguage }).Patterns[0];
            var timeFormatter = new DateTimeFormatter("shorttime", new[] { formattingLanguage }).Patterns[0];
            var fullFormatter = new DateTimeFormatter(dateFormatter + " " + timeFormatter);
            return fullFormatter;
        }

        public static DateTimeFormatter GetTimeFormatter()
        {
            string formattingLanguage = SelectFormattingLanguage();
            var timeFormatter = new DateTimeFormatter("shorttime", new[] { formattingLanguage }).Patterns[0];
            var fullFormatter = new DateTimeFormatter(timeFormatter);
            return fullFormatter;
        }
    }
}
