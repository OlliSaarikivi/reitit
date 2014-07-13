using Reitit.API;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Shapes = Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Reitit
{
    public partial class ExpandingRouteControl : UserControl
    {
        private static Brush _transparent;
        private static Brush _accentBrush;
        private static Brush _foregroundBrush;
        private static Brush _backgroundBrush;
        private static Brush _subtleBrush;
        private static FontFamily _boldFont;
        private static double _extraLargeFontSize;
        private static double _largeFontSize;
        private static double _normalFontSize;
        private static double _smallFontSize;
        private static double _tinyFontSize;
        private static double _timeStringMaxLength;

        public CompoundRoute Route
        {
            get { return (CompoundRoute)GetValue(RouteProperty); }
            set { SetValue(RouteProperty, value); }
        }
        public static readonly DependencyProperty RouteProperty =
            DependencyProperty.Register("Route", typeof(CompoundRoute), typeof(ExpandingRouteControl), new PropertyMetadata(null, OnRouteChanged));
        private static void OnRouteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ExpandingRouteControl)d;
            control.Populate(e.NewValue as CompoundRoute);
        }

        public string From
        {
            get { return (string)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(string), typeof(ExpandingRouteControl), new PropertyMetadata(null));

        public string To
        {
            get { return (string)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(string), typeof(ExpandingRouteControl), new PropertyMetadata(null));

        public bool Expanded
        {
            get { return (bool)GetValue(ExpandedProperty); }
            set { SetValue(ExpandedProperty, value); }
        }
        public static readonly DependencyProperty ExpandedProperty =
            DependencyProperty.Register("Expanded", typeof(bool), typeof(ExpandingRouteControl), new PropertyMetadata(false, OnExpandedChanged));
        private static void OnExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ExpandingRouteControl)d;
            if (control.Expanded)
            {
                if (control.ExpandedRoot.Children.Count == 0)
                {
                    control.PopulateExpanded(control.Route);
                    control.Root.Children.Add(control.ExpandedRoot);
                }
                control.ExpandedRoot.Visibility = Visibility.Visible;
                //control.MinimizedRoot.Opacity = 0;
                control._minimizeBoard.Stop();
                control._maximizeBoard.Begin();
            }
            else
            {
                control.ExpandedRoot.Visibility = Visibility.Collapsed;
                //control.MinimizedRoot.Opacity = 1;
                control._maximizeBoard.Stop();
                control._minimizeBoard.Begin();
            }
        }

        private Storyboard _minimizeBoard, _maximizeBoard;

        private Grid Root { get; set; }
        private Grid MinimizedRoot { get; set; }
        private Grid ExpandedRoot { get; set; }
        private ScaleTransform ExpandTransform { get; set; }

        static ExpandingRouteControl()
        {
            _transparent = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            _accentBrush = (Brush)App.Current.Resources["PhoneAccentBrush"];

            var foregroundColor = (Color)App.Current.Resources["PhoneForegroundColor"];
            var backgroundColor = (Color)App.Current.Resources["PhoneBackgroundColor"];
            _foregroundBrush = new SolidColorBrush(foregroundColor.FlattenOn(backgroundColor));

            _backgroundBrush = (Brush)App.Current.Resources["PhoneBackgroundBrush"];
            _subtleBrush = (Brush)App.Current.Resources["PhoneSubtleBrush"];
            _boldFont = (FontFamily)App.Current.Resources["PhoneFontFamilySemiBold"];
            _extraLargeFontSize = (double)App.Current.Resources["PhoneFontSizeExtraLarge"];
            _largeFontSize = (double)App.Current.Resources["PhoneFontSizeLarge"];
            _normalFontSize = (double)App.Current.Resources["PhoneFontSizeNormal"];
            _smallFontSize = (double)App.Current.Resources["PhoneFontSizeSmall"];
            _tinyFontSize = 16;

            var textBlock = new TextBlock { FontSize = _largeFontSize };
            textBlock.Text = Utils.CurrentCultureUsesTwentyFourHourClock() ? "88:88" : "12:88 AM";
            _timeStringMaxLength = textBlock.ActualWidth;
        }

        public ExpandingRouteControl()
        {
            InitializeComponent();
        }

        private void Populate(CompoundRoute route)
        {
            Root = new Grid();
            Content = Root;
            var rootTilt = new ListViewItem();
            Root.Children.Add(rootTilt);
            MinimizedRoot = new Grid();
            PopulateMinimzied(route);
            ExpandedRoot = new Grid();
            rootTilt.Content = MinimizedRoot;
            ExpandTransform = new ScaleTransform();
            ExpandedRoot.RenderTransform = ExpandTransform;
            ExpandTransform.CenterY = 0;
            if (Expanded)
            {
                MinimizedRoot.Opacity = 0;
                PopulateExpanded(route);
                Root.Children.Add(ExpandedRoot);
            }
            else
            {
                ExpandedRoot.Opacity = 0;
                ExpandedRoot.Visibility = Visibility.Collapsed;
                ExpandTransform.ScaleY = 0.51;
            }

            var fadeEase = new ExponentialEase();
            fadeEase.EasingMode = EasingMode.EaseOut;
            fadeEase.Exponent = 3;

            var dur = TimeSpan.FromSeconds(0.2);

            var fadeExpIn = new DoubleAnimation();
            fadeExpIn.To = 1;
            fadeExpIn.Duration = dur;
            fadeExpIn.EasingFunction = fadeEase;
            Storyboard.SetTargetProperty(fadeExpIn, "Opacity");
            Storyboard.SetTarget(fadeExpIn, ExpandedRoot);
            var maximizeExp = new DoubleAnimation();
            maximizeExp.To = 1;
            maximizeExp.Duration = dur;
            maximizeExp.EasingFunction = fadeEase;
            Storyboard.SetTargetProperty(maximizeExp, "ScaleY");
            Storyboard.SetTarget(maximizeExp, ExpandTransform);
            var fadeMinOut = new DoubleAnimation();
            fadeMinOut.To = 0;
            fadeMinOut.Duration = dur;
            fadeMinOut.EasingFunction = fadeEase;
            Storyboard.SetTargetProperty(fadeMinOut, "Opacity");
            Storyboard.SetTarget(fadeMinOut, MinimizedRoot);

            _maximizeBoard = new Storyboard();
            _maximizeBoard.Children.Add(fadeExpIn);
            _maximizeBoard.Children.Add(maximizeExp);
            _maximizeBoard.Children.Add(fadeMinOut);

            var fadeMinIn = new DoubleAnimation();
            fadeMinIn.To = 1;
            fadeMinIn.Duration = dur;
            fadeMinIn.EasingFunction = fadeEase;
            Storyboard.SetTargetProperty(fadeMinIn, "Opacity");
            Storyboard.SetTarget(fadeMinIn, MinimizedRoot);
            var minimizeExp = new DoubleAnimation();
            minimizeExp.To = 0.51;
            minimizeExp.Duration = dur;
            minimizeExp.EasingFunction = fadeEase;
            Storyboard.SetTargetProperty(minimizeExp, "ScaleY");
            Storyboard.SetTarget(minimizeExp, ExpandTransform);
            var fadeExpOut = new DoubleAnimation();
            fadeExpOut.To = 0;
            fadeExpOut.Duration = dur;
            fadeExpOut.EasingFunction = fadeEase;
            Storyboard.SetTargetProperty(fadeExpOut, "Opacity");
            Storyboard.SetTarget(fadeExpOut, ExpandedRoot);

            _minimizeBoard = new Storyboard();
            _minimizeBoard.Children.Add(fadeMinIn);
            _minimizeBoard.Children.Add(minimizeExp);
            _minimizeBoard.Children.Add(fadeExpOut);
        }

        private void PopulateMinimzied(CompoundRoute route)
        {
            MinimizedRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_timeStringMaxLength + 12) });
            MinimizedRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            MinimizedRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_timeStringMaxLength + 12) });

            { // Add departure time
                var loc = route.Routes[0].Legs[0].Locs[0];
                string timeString = Utils.ToShortTimeString(loc.DepTime);
                var textBlock = new TextBlock
                {
                    Text = timeString,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = _largeFontSize,
                };
                Grid.SetColumn(textBlock, 0);
                MinimizedRoot.Children.Add(textBlock);
            }

            { // Add arrival time
                var loc = route.Routes.LastElement().Legs.LastElement().Locs.LastElement();
                string timeString = Utils.ToShortTimeString(loc.ArrTime);
                var textBlock = new TextBlock
                {
                    Text = timeString,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FontSize = _largeFontSize,
                };
                Grid.SetColumn(textBlock, 2);
                MinimizedRoot.Children.Add(textBlock);
            }

            var vehiclesStack = new WrapGrid
            {
                Orientation = Orientation.Horizontal
            };
            Grid.SetColumn(vehiclesStack, 1);
            MinimizedRoot.Children.Add(vehiclesStack);
            foreach (var partRoute in route.Routes)
            {
                for (int i = 0; i < partRoute.Legs.Length; ++i)
                {
                    Leg leg = partRoute.Legs[i];

                    var detailsStack = new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(6, 6, 6, 6)
                    };
                    vehiclesStack.Children.Add(detailsStack);

                    var iconControl = new ColorizedIconControl
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        IconBackground = Utils.GetStrokeForType(leg.Type),
                        Icon = Utils.GetIconForType(leg.Type),
                    };
                    detailsStack.Children.Add(iconControl);

                    var textContainer = new Border
                    {
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(0, 0, -12, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        MinWidth = 35, // The size of the icon above
                    };
                    detailsStack.Children.Add(textContainer);

                    var detailsText = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontSize = _tinyFontSize,
                        Margin = new Thickness(0, 2, 0, 0),
                    };
                    textContainer.Child = detailsText;

                    if (leg.Type == "walk")
                    {
                        detailsText.Text = Utils.FormatDistanceConserveSpace(leg.Length);
                        detailsText.Foreground = _subtleBrush;
                    }
                    else
                    {
                        detailsText.Text = leg.Line != null ? leg.Line.ShortName : "";
                    }
                }
            }
        }

        private void PopulateExpanded(CompoundRoute route)
        {
            ExpandedRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            ExpandedRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            ExpandedRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            int row = -1;
            int locationNum = 1;
            foreach (var partRoute in route.Routes)
            {
                for (int i = 0; i < partRoute.Legs.Length; ++i)
                {
                    Leg leg = partRoute.Legs[i];
                    bool isFirst = (i == 0);
                    bool isLast = (i == (partRoute.Legs.Length - 1));

                    bool showWait;
                    {
                        ExpandedRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        row += 1;

                        var waitTime = AddTime(leg, row, start: true, arrival: true, endpoint: isFirst || isLast);

                        showWait = waitTime > TimeSpan.FromMinutes(1);

                        AddDotLine(leg, isFirst, false, true, false, row);
                        locationNum += 1;

                        if (leg.Locs.Length > 0)
                        {
                            var location = leg.Locs[0];
                            AddLocationDetails(location, isFirst, false, row);
                        }

                        if (showWait)
                        {
                            ExpandedRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            row += 1;

                            AddDotLine(leg, false, false, false, true, row);

                            AddWaitDetails(waitTime, row);

                            ExpandedRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            row += 1;

                            AddTime(leg, row, start: true, arrival: false, endpoint: isFirst || isLast);

                            AddDotLine(leg, false, false, true, true, row);
                        }
                    }

                    {
                        ExpandedRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        row += 1;

                        AddDotLine(leg, false, false, false, true, row);

                        AddLegDetails(leg, showWait, row);
                    }

                    if (isLast)
                    {
                        ExpandedRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        row += 1;

                        AddTime(leg, row, start: false, arrival: true, endpoint: isFirst || isLast);

                        AddDotLine(leg, false, isLast, true, false, row);
                        locationNum += 1;

                        if (leg.Locs.Length > 0)
                        {
                            var location = leg.Locs[leg.Locs.Length - 1];
                            AddLocationDetails(location, isFirst, isLast, row);
                        }
                    }
                }
            }
        }

        private TimeSpan AddTime(Leg leg, int row, bool start, bool arrival, bool endpoint)
        {
            TimeSpan waitDuration = TimeSpan.Zero;
            string timeString = null;
            if (leg.Locs.Length > 0)
            {
                if (start)
                {
                    var loc = leg.Locs[0];
                    if (arrival)
                    {
                        timeString = Utils.ToShortTimeString(loc.ArrTime);
                    }
                    else
                    {
                        timeString = Utils.ToShortTimeString(loc.DepTime);
                    }
                    waitDuration = loc.DepTime - loc.ArrTime;
                }
                else
                {
                    var loc = leg.Locs[leg.Locs.Length - 1];
                    if (arrival)
                    {
                        timeString = Utils.ToShortTimeString(loc.ArrTime);
                    }
                    else
                    {
                        timeString = Utils.ToShortTimeString(loc.DepTime);
                    }
                    waitDuration = loc.DepTime - loc.ArrTime;
                }
            }
            var timeTextBlock = new TextBlock
            {
                Text = timeString,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = endpoint ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                FontSize = endpoint ? _largeFontSize : _normalFontSize,
                Margin = new Thickness(0, 12, 12, 12)
            };
            Grid.SetRow(timeTextBlock, row);
            Grid.SetColumn(timeTextBlock, 0);
            ExpandedRoot.Children.Add(timeTextBlock);

            return waitDuration;
        }

        private void AddDotLine(Leg leg, bool isFirst, bool isLast, bool isLocation, bool isSmall, int row)
        {
            var canvas = new Canvas { Width = 40 };

            Grid.SetRow(canvas, row);
            Grid.SetColumn(canvas, 1);
            ExpandedRoot.Children.Add(canvas);

            var line = new Shapes.Line { Stroke = _accentBrush, StrokeThickness = 4 };
            canvas.SizeChanged += (s, e) =>
            {
                double x = e.NewSize.Width / 2;
                line.X1 = x;
                line.X2 = x;

                if (isFirst)
                {
                    line.Y1 = e.NewSize.Height / 2;
                }
                else
                {
                    line.Y1 = 0;
                }

                if (isLast)
                {
                    line.Y2 = e.NewSize.Height / 2;
                }
                else
                {
                    line.Y2 = e.NewSize.Height;
                }
            };
            canvas.Children.Add(line);

            if (isLocation)
            {
                double diameter = isSmall ? 10 : 14;

                {
                    var circle = new Shapes.Ellipse { Width = diameter, Height = diameter, Fill = _foregroundBrush };
                    canvas.SizeChanged += (s, e) =>
                    {
                        double left = e.NewSize.Width / 2 - circle.Width / 2;
                        double top = e.NewSize.Height / 2 - circle.Height / 2;
                        Canvas.SetLeft(circle, left);
                        Canvas.SetTop(circle, top);
                    };
                    canvas.Children.Add(circle);
                }
            }
        }

        private void AddLocationDetails(LegLocation location, bool isFirst, bool isLast, int row)
        {
            var locationName = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontSize = _largeFontSize,
                Margin = new Thickness(12, 6, 0, 6)
            };

            if (isFirst || isLast)
            {
                var binding = new Binding {
                    Path = new PropertyPath(isFirst ? "From" : "To"),
                    Converter = new DefaultIfNullConverter(),
                    ConverterParameter = location.Name,
                    Source = this,
                    Mode = BindingMode.OneWay,
                };
                BindingOperations.SetBinding(locationName, TextBlock.TextProperty, binding);
            }
            else
            {
                locationName.Text = location.Name;
            }

            Grid.SetRow(locationName, row);
            Grid.SetColumn(locationName, 2);
            ExpandedRoot.Children.Add(locationName);
        }

        private void AddWaitDetails(TimeSpan waitTime, int row)
        {
            string waitText = null;

            if (waitTime.TotalHours < 1)
            {
                waitText = waitTime.ToString(Utils.GetString("RouteControlMinutesWaitFormat"));
            }
            else
            {
                waitText = waitTime.ToString(Utils.GetString("RouteControlHoursWaitFormat"));
            }

            var lengthTextBlock = new TextBlock
            {
                Text = waitText,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = _normalFontSize,
                Margin = new Thickness(12, 0, 0, 0),
                TextWrapping = TextWrapping.Wrap,
            };
            Grid.SetRow(lengthTextBlock, row);
            Grid.SetColumn(lengthTextBlock, 2);
            ExpandedRoot.Children.Add(lengthTextBlock);
        }

        private void AddLegDetails(Leg leg, bool hasTime, int row)
        {
            if (leg.Type == "walk")
            {
                var detailsStack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(12, 6, 0, 6)
                };

                var iconControl = new ColorizedIconControl
                {
                    IconBackground = Utils.GetStrokeForType(leg.Type),
                    Icon = Utils.GetIconForType(leg.Type)
                };
                detailsStack.Children.Add(iconControl);

                var lengthTextBlock = new TextBlock
                {
                    Text = Utils.FormatDistance(leg.Length),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    FontSize = _normalFontSize,
                    Foreground = _subtleBrush,
                    Margin = new Thickness(12, 0, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                };
                detailsStack.Children.Add(lengthTextBlock);

                Grid.SetRow(detailsStack, hasTime ? row - 1 : row);
                Grid.SetColumn(detailsStack, 2);
                detailsStack.Name = Guid.NewGuid().ToString();
                ExpandedRoot.Children.Add(detailsStack);
            }
            else
            {

                var tilt = new ListViewItem
                {

                };

                var detailsStack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(12, 6, 0, 6),
                    Background = _transparent,
                };

                tilt.Content = detailsStack;

                var iconControl = new ColorizedIconControl
                {
                    IconBackground = Utils.GetStrokeForType(leg.Type),
                    Icon = Utils.GetIconForType(leg.Type),
                };
                detailsStack.Children.Add(iconControl);

                Brush foreground;
                string mode;
                if (leg.Line != null)
                {
                    foreground = _accentBrush;
                    mode = leg.Line.ShortName;
                }
                else
                {
                    foreground = _foregroundBrush;
                    mode = "Other";
                }

                var modeTextBlock = new TextBlock
                {
                    Text = mode,
                    Foreground = foreground,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    FontSize = _largeFontSize,
                    Margin = new Thickness(12, 0, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                };
                detailsStack.Children.Add(modeTextBlock);

                var stopsStack = new StackPanel
                {
                    Margin = new Thickness(0, 0, 0, 6)
                };

                for (int i = 1; i < leg.Locs.Length - 1; i += 1)
                {
                    var loc = leg.Locs[i];

                    string stopText = Utils.ToShortTimeString(loc.DepTime) + " " + loc.Name;

                    var stopTextBlock = new TextBlock
                    {
                        Text = stopText,
                        FontSize = _normalFontSize,
                        Margin = new Thickness(12, 0, 0, 0),
                        TextWrapping = TextWrapping.Wrap,
                    };
                    stopsStack.Children.Add(stopTextBlock);
                }

                if (hasTime)
                {
                    Grid.SetRow(tilt, row - 1);
                    Grid.SetColumn(tilt, 2);
                    ExpandedRoot.Children.Add(tilt);

                    Grid.SetRow(stopsStack, row);
                    Grid.SetColumn(stopsStack, 2);
                    ExpandedRoot.Children.Add(stopsStack);
                }
                else
                {
                    var allPanel = new StackPanel
                    {

                    };

                    allPanel.Children.Add(tilt);

                    allPanel.Children.Add(stopsStack);

                    Grid.SetRow(allPanel, row);
                    Grid.SetColumn(allPanel, 2);
                    ExpandedRoot.Children.Add(allPanel);
                }

                //tilt.Tap += (s, e) =>
                //{
                //    if (OnTapLine != null && leg.Line != null)
                //    {
                //        OnTapLine(this, leg.Line);
                //    }
                //};
            }
        }
    }
}
