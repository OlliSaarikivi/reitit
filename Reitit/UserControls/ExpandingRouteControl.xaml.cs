using GalaSoft.MvvmLight.Command;
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
using Windows.UI.Xaml.Shapes;
using Shapes = Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

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
            if (e.NewValue != null)
            {
                control.Populate(e.NewValue as CompoundRoute);
            }
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

        public Leg FocusedLeg
        {
            get { return (Leg)GetValue(FocusedLegProperty); }
            set { SetValue(FocusedLegProperty, value); }
        }
        public static readonly DependencyProperty FocusedLegProperty =
            DependencyProperty.Register("FocusedLeg", typeof(Leg), typeof(ExpandingRouteControl), new PropertyMetadata(null));

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
                    control.Root.Children.Add(control.ExpandedNoTilt);
                }
                control.ExpandedNoTilt.Visibility = Visibility.Visible;
                //control.MinimizedRoot.Opacity = 0;
                control._minimizeBoard.Stop();
                control._maximizeBoard.Begin();
            }
            else
            {
                control.ExpandedNoTilt.Visibility = Visibility.Collapsed;
                //control.MinimizedRoot.Opacity = 1;
                control._maximizeBoard.Stop();
                control._minimizeBoard.Begin();
                control.FocusedLeg = null;
            }
        }

        private Storyboard _minimizeBoard, _maximizeBoard;

        private Grid Root { get; set; }
        private Grid MinimizedRoot { get; set; }
        private Grid ExpandedRoot { get; set; }
        private ListViewItem ExpandedNoTilt { get; set; }
        private ScaleTransform ExpandTransform { get; set; }

        static ExpandingRouteControl()
        {
            _transparent = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            _accentBrush = (Brush)App.Current.Resources["PhoneAccentBrush"];
            _foregroundBrush = (Brush)App.Current.Resources["PhoneForegroundBrush"];
            _backgroundBrush = (Brush)App.Current.Resources["PhoneBackgroundBrush"];
            _subtleBrush = (Brush)App.Current.Resources["PhoneSubtleBrush"];
            _boldFont = (FontFamily)App.Current.Resources["PhoneFontFamilySemiBold"];
            _extraLargeFontSize = (double)App.Current.Resources["TextStyleExtraLargeFontSize"];
            _largeFontSize = (double)App.Current.Resources["TextStyleLargeFontSize"];
            _normalFontSize = (double)App.Current.Resources["TextStyleMediumFontSize"];
            _smallFontSize = (double)App.Current.Resources["TextStyleSmallFontSize"];
            _tinyFontSize = 16;

            var textBlock = new TextBlock { FontSize = _extraLargeFontSize };
            textBlock.Text = Utils.CurrentCultureUsesTwentyFourHourClock() ? "88:88" : "10:88 AM";
            textBlock.Measure(new Size(1000, 1000));
            _timeStringMaxLength = textBlock.DesiredSize.Width;
        }

        public ExpandingRouteControl()
        {
            InitializeComponent();
        }

        private void Populate(CompoundRoute route)
        {
            Root = new Grid();
            Content = Root;
            MinimizedRoot = new Grid();
            PopulateMinimzied(route);
            ExpandedRoot = new Grid();
            ExpandedNoTilt = new ListViewItem
            {
                Content = ExpandedRoot,
                Style = (Style)App.Current.Resources["NonTiltListViewItemStyle"],
            };
            Root.Children.Add(MinimizedRoot);
            ExpandTransform = new ScaleTransform();
            ExpandedNoTilt.RenderTransform = ExpandTransform;
            ExpandTransform.CenterY = 0;
            if (Expanded)
            {
                MinimizedRoot.Opacity = 0;
                PopulateExpanded(route);
                Root.Children.Add(ExpandedNoTilt);
            }
            else
            {
                ExpandedNoTilt.Opacity = 0;
                ExpandedNoTilt.Visibility = Visibility.Collapsed;
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
            Storyboard.SetTarget(fadeExpIn, ExpandedNoTilt);
            var maximizeExp = new DoubleAnimation();
            maximizeExp.To = 1;
            maximizeExp.Duration = dur;
            maximizeExp.EasingFunction = fadeEase;
            Storyboard.SetTargetProperty(maximizeExp, "ScaleY");
            Storyboard.SetTarget(maximizeExp, ExpandTransform);
            var fadeMinOut = new DoubleAnimation();
            fadeMinOut.To = 0;
            fadeMinOut.Duration = TimeSpan.Zero;
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
            Storyboard.SetTarget(fadeExpOut, ExpandedNoTilt);

            _minimizeBoard = new Storyboard();
            _minimizeBoard.Children.Add(fadeMinIn);
            _minimizeBoard.Children.Add(minimizeExp);
            _minimizeBoard.Children.Add(fadeExpOut);
        }

        private void PopulateMinimzied(CompoundRoute route)
        {
            MinimizedRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            MinimizedRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            //MinimizedRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_timeStringMaxLength + 10) });
            //MinimizedRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var detailsRoot = new Grid();
            Grid.SetRow(detailsRoot, 0);
            MinimizedRoot.Children.Add(detailsRoot);

            detailsRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_timeStringMaxLength + 10) });
            detailsRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            detailsRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_timeStringMaxLength + 10) });

            { // Add departure time
                var loc = route.Routes[0].Legs[0].Locs[0];
                string timeString = Utils.ToShortTimeString(loc.DepTime);
                var textBlock = new TextBlock
                {
                    Text = timeString,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    TextLineBounds = TextLineBounds.TrimToBaseline,
                    Margin = new Thickness(0, 0, 0, 5),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = _extraLargeFontSize,
                };
                Grid.SetColumn(textBlock, 0);
                detailsRoot.Children.Add(textBlock);
            }

            // Add route details
            string description = "";
            if (route.Routes[0].Legs[0].Type == "walk")
            {
                var firstRoute = route.Routes[0];
                for (int i = 1; i < firstRoute.Legs.Length; ++i)
                {
                    if (firstRoute.Legs[i].Type != "walk")
                    {
                        var atStop = firstRoute.Legs[i].Locs[0].DepTime;
                        description += string.Format(Utils.GetString("RouteDetailsOnStopFormat"), Utils.ToShortTimeString(atStop)) + " ";
                        break;
                    }
                }
            }
            TimeSpan totalTime = TimeSpan.Zero;
            foreach (var subRoute in route.Routes)
            {
                totalTime += subRoute.Duration;
            }
            description += string.Format(Utils.GetString("RouteDetailsShortTimeFormat"), Utils.FormatTimeSpan(totalTime));
            var detailsBlock = new TextBlock
            {
                Text = description,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextLineBounds = TextLineBounds.TrimToBaseline,
                Margin = new Thickness(0,0,0,5),
                FontSize = _normalFontSize,
                Foreground = _subtleBrush,
                TextWrapping = TextWrapping.WrapWholeWords,
            };
            Grid.SetColumn(detailsBlock, 1);
            detailsRoot.Children.Add(detailsBlock);

            { // Add arrival time
                var loc = route.Routes.LastElement().Legs.LastElement().Locs.LastElement();
                string timeString = Utils.ToShortTimeString(loc.ArrTime);
                var textBlock = new TextBlock
                {
                    Text = timeString,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    TextLineBounds = TextLineBounds.TrimToBaseline,
                    Margin = new Thickness(0, 0, 0, 5),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FontSize = _extraLargeFontSize,
                };
                Grid.SetColumn(textBlock, 2);
                detailsRoot.Children.Add(textBlock);
            }

            // Add vehicle icons etc.
            var vehiclesStack = new VariableSizedWrapGrid
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(-5, 0, -5, 0),
            };
            Grid.SetRow(vehiclesStack, 1);
            MinimizedRoot.Children.Add(vehiclesStack);
            foreach (var partRoute in route.Routes)
            {
                for (int i = 0; i < partRoute.Legs.Length; ++i)
                {
                    Leg leg = partRoute.Legs[i];

                    var detailsStack = new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5, 5, 5, 5)
                    };
                    vehiclesStack.Children.Add(detailsStack);

                    var iconControl = new ModeIcon
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Type = leg.Type,
                    };
                    detailsStack.Children.Add(iconControl);

                    var textContainer = new Border
                    {
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(0, 0, -10, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        MinWidth = 40, // The size of the icon above
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

            ExpandedRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var topMiscGrid = new Grid();
            topMiscGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topMiscGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            topMiscGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            topMiscGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            topMiscGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumnSpan(topMiscGrid, 3);
            ExpandedRoot.Children.Add(topMiscGrid);

            // Add route details
            double totalLength = 0, walkLength = 0;
            TimeSpan totalTime = TimeSpan.Zero;
            TimeSpan totalWaitTime = TimeSpan.Zero;
            foreach (var subRoute in route.Routes)
            {
                totalLength += subRoute.Length;
                totalTime += subRoute.Duration;
                foreach (var leg in subRoute.Legs)
                {
                    if (leg.Type == "walk")
                    {
                        walkLength += leg.Length;
                    }
                    var legWait = leg.Locs[0].DepTime - leg.Locs[0].ArrTime;
                    if (legWait > TimeSpan.FromMinutes(1))
                        totalWaitTime += legWait;
                }
            }
            string timeDescription;
            if (totalWaitTime.TotalMinutes < 1)
            {
                timeDescription = string.Format(Utils.GetString("RouteDetailsTimesFormatNoWaiting"), Utils.FormatTimeSpan(totalTime));
            }
            else
            {
                timeDescription = string.Format(Utils.GetString("RouteDetailsTimesFormatWithWaiting"), Utils.FormatTimeSpan(totalTime), Utils.FormatTimeSpan(totalWaitTime));
            }
            var timeBlock = new TextBlock
            {
                Text = timeDescription,
                VerticalAlignment = VerticalAlignment.Bottom,
                FontSize = _normalFontSize,
                Foreground = _subtleBrush,
                TextWrapping = TextWrapping.WrapWholeWords,
            };
            Grid.SetRow(timeBlock, 1);
            topMiscGrid.Children.Add(timeBlock);
            string distanceDescription = string.Format(Utils.GetString("RouteDetailsDistancesFormat"), Utils.FormatDistance(totalLength), Utils.FormatDistance(walkLength));
            var distanceBlock = new TextBlock
            {
                Text = distanceDescription,
                VerticalAlignment = VerticalAlignment.Bottom,
                FontSize = _normalFontSize,
                Foreground = _subtleBrush,
                TextWrapping = TextWrapping.WrapWholeWords,
            };
            Grid.SetRow(distanceBlock, 2);
            topMiscGrid.Children.Add(distanceBlock);

            // Add focus all button
            var focusAll = new FocusAllButton
            {
                Margin = new Thickness(10, -7.5, 0, -7.5),
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            focusAll.FocusAll += () =>
            {
                FocusedLeg = null;
            };
            var binding = new Binding
            {
                Path = new PropertyPath("FocusedLeg"),
                Converter = new NotNullConverter(),
                Source = this,
            };
            focusAll.SetBinding(FocusAllButton.ShownProperty, binding);
            Grid.SetRowSpan(focusAll, 3);
            Grid.SetColumn(focusAll, 1);
            topMiscGrid.Children.Add(focusAll);

            int row = 0;
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

                        var waitTime = AddTime(leg, row, start: true, arrival: true, endpoint: isFirst);

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

                            AddTime(leg, row, start: true, arrival: false);

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

                        AddTime(leg, row, start: false, arrival: true, endpoint: true);

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

        private TimeSpan AddTime(Leg leg, int row, bool start, bool arrival, bool endpoint = false)
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
                FontSize = endpoint ? _extraLargeFontSize : _largeFontSize,
                Margin = new Thickness(0, 10, 0, 10)
            };
            Grid.SetRow(timeTextBlock, row);
            Grid.SetColumn(timeTextBlock, 0);
            ExpandedRoot.Children.Add(timeTextBlock);

            return waitDuration;
        }

        private void AddDotLine(Leg leg, bool isFirst, bool isLast, bool isLocation, bool isSmall, int row)
        {
            var canvas = new Canvas
            {
                Width = 40,
                Background = _transparent,
            };

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

            var legFocuser = new Rectangle
            {
                Fill = _transparent,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            Grid.SetRow(legFocuser, row);
            Grid.SetColumn(legFocuser, 0);
            Grid.SetColumnSpan(legFocuser, 2);
            ExpandedRoot.Children.Add(legFocuser);

            legFocuser.Tapped += (s, e) =>
            {
                FocusedLeg = leg;
            };
        }

        private void AddLocationDetails(LegLocation location, bool isFirst, bool isLast, int row)
        {
            var locationName = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.WrapWholeWords,
                FontSize = _extraLargeFontSize,
                Margin = new Thickness(0, 10, 0, 10)
            };

            if (isFirst || isLast)
            {
                var binding = new Binding
                {
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
                Margin = new Thickness(0, 0, 0, 5),
                TextWrapping = TextWrapping.WrapWholeWords,
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
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var iconControl = new ModeIcon
                {
                    Type = leg.Type,
                };
                detailsStack.Children.Add(iconControl);

                var lengthTextBlock = new TextBlock
                {
                    Text = Utils.FormatDistance(leg.Length),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = _normalFontSize,
                    Foreground = _subtleBrush,
                    Margin = new Thickness(10, 0, 0, 0),
                    TextWrapping = TextWrapping.WrapWholeWords,
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
                    Margin = new Thickness(0, 5, 0, 5),
                    Background = _transparent,
                };

                tilt.Content = detailsStack;

                var iconControl = new ModeIcon
                {
                    Type = leg.Type,
                };
                detailsStack.Children.Add(iconControl);

                Brush foreground;
                string mode;
                if (leg.Line != null)
                {
                    foreground = _accentBrush;
                    mode = Utils.GetDescriptiveShortLineName(leg.Line);
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
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = _largeFontSize,
                    Margin = new Thickness(10, 0, 0, 0),
                    TextWrapping = TextWrapping.WrapWholeWords,
                };
                detailsStack.Children.Add(modeTextBlock);

                var stopsStack = new StackPanel
                {
                    Margin = new Thickness(0, 5, 0, 5)
                };
                for (int i = 1; i < leg.Locs.Length - 1; i += 1)
                {
                    var loc = leg.Locs[i];

                    string stopText = Utils.ToShortTimeString(loc.DepTime) + " " + loc.Name;

                    var stopTextBlock = new TextBlock
                    {
                        Text = stopText,
                        FontSize = _normalFontSize,
                        Margin = new Thickness(0, 0, 0, 0),
                        TextWrapping = TextWrapping.WrapWholeWords,
                    };
                    stopsStack.Children.Add(stopTextBlock);
                }
                if (stopsStack.Children.Count == 0)
                {
                    stopsStack.Visibility = Visibility.Collapsed;
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
