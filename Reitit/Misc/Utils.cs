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

namespace Reitit
{
    static class Utils
    {
        public static readonly double MapEpsilon = 0.0000001;

        public static readonly Color FromColor = Color.FromArgb(255, 0, 160, 0);
        public static readonly Color ToColor = Color.FromArgb(255, 160, 15, 0);

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

        public static readonly Brush TrainStroke = new SolidColorBrush(Color.FromArgb(255, 233, 0, 26));
        public static readonly Brush FerryStroke = new SolidColorBrush(Color.FromArgb(255, 90, 197, 216));
        public static readonly Brush TramStroke = new SolidColorBrush(Color.FromArgb(255, 0, 175, 46));
        public static readonly Brush MetroStroke = new SolidColorBrush(Color.FromArgb(255, 238, 62, 12));
        public static readonly Brush BusStroke = new SolidColorBrush(Color.FromArgb(255, 25, 54, 149));
        public static readonly Brush WalkStroke = new SolidColorBrush(Color.FromArgb(255, 0, 99, 255));

        public static Brush GetStrokeForType(string type)
        {
            if (type == "walk")
            {
                return WalkStroke;
            }
            else if (type == "2")
            {
                return TramStroke;
            }
            else if (type == "6")
            {
                return MetroStroke;
            }
            else if (type == "7")
            {
                return FerryStroke;
            }
            else if (type == "12")
            {
                return TrainStroke;
            }
            else // Buses and everything else
            {
                return BusStroke;
            }
        }

        public static ImageSource GetIconForType(string type)
        {
            throw new NotImplementedException();
            //if (type == "walk")
            //{
            //    return (ImageSource)new ImageSourceConverter().ConvertFromString("/Assets/Walk.png");
            //}
            //else if (type == "2")
            //{
            //    return (ImageSource)new ImageSourceConverter().ConvertFromString("/Assets/Tram.png");
            //}
            //else if (type == "6")
            //{
            //    return (ImageSource)new ImageSourceConverter().ConvertFromString("/Assets/Metro.png");
            //}
            //else if (type == "7")
            //{
            //    return (ImageSource)new ImageSourceConverter().ConvertFromString("/Assets/Ferry.png");
            //}
            //else if (type == "12")
            //{
            //    return (ImageSource)new ImageSourceConverter().ConvertFromString("/Assets/Train.png");
            //}
            //else // Buses and everything else
            //{
            //    return (ImageSource)new ImageSourceConverter().ConvertFromString("/Assets/Bus.png");
            //}
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

        //public static void ShowMessageFlyout(string message, string title)
        //{
        //    var flyout = new Flyout
        //    {
        //        Placement = FlyoutPlacementMode.Top,
        //        FlyoutPresenterStyle = (Style)Application.Current.Resources["NoScrollFlyoutPresenterStyle"],
        //        Content = new MessageFlyoutContent
        //        {
        //            Title = title,
        //            Message = message,
        //        },
        //    };
        //    flyout.ShowAt((Frame)Window.Current.Content);
        //}

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
    }
}
