using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ReittiAPI;
using System.Windows.Media;
using Shapes = System.Windows.Shapes;
using GalaSoft.MvvmLight;
using System.Windows.Data;
using Reitit.Resources;

namespace Reitit
{
    public partial class ExpandingRouteControl : UserControl
    {
        private Brush _transparent;
        private Brush _maximizerMask;
        private Brush _accentBrush;
        private Brush _foregroundBrush;
        private Brush _backgroundBrush;
        private Brush _subtleBrush;
        private FontFamily _boldFont;
        private double _extraLargeFontSize;
        private double _largeFontSize;
        private double _normalFontSize;
        private double _smallFontSize;

        public CompoundRoute Route
        {
            get { return (CompoundRoute)GetValue(RouteProperty); }
            set { SetValue(RouteProperty, value); }
        }
        public static readonly DependencyProperty RouteProperty =
            DependencyProperty.Register("Route", typeof(CompoundRoute), typeof(ExpandingRouteControl), new PropertyMetadata(null, OnRouteChanged));

        public string From
        {
            get { return (string)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(string), typeof(ExpandingRouteControl), new PropertyMetadata(0));

        public string To
        {
            get { return (string)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(string), typeof(ExpandingRouteControl), new PropertyMetadata(0));

        private static void OnRouteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ExpandingRouteControl;
            if (control != null)
            {
                control.Populate((CompoundRoute)e.NewValue);
            }
        }

        public Grid Root { get; set; }

        public ExpandingRouteControl()
        {
            InitializeComponent();

            _transparent = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            _accentBrush = (Brush)Resources["PhoneAccentBrush"];

            var foregroundColor = (Color)App.Current.Resources["PhoneForegroundColor"];
            var backgroundColor = (Color)App.Current.Resources["PhoneBackgroundColor"];
            _foregroundBrush = new SolidColorBrush(foregroundColor.FlattenOn(backgroundColor));

            _backgroundBrush = (Brush)Resources["PhoneBackgroundBrush"];
            _subtleBrush = (Brush)Resources["PhoneSubtleBrush"];
            _boldFont = (FontFamily)Resources["PhoneFontFamilySemiBold"];
            _extraLargeFontSize = (double)Resources["PhoneFontSizeExtraLarge"];
            _largeFontSize = (double)Resources["PhoneFontSizeLarge"];
            _normalFontSize = (double)Resources["PhoneFontSizeNormal"];
            _smallFontSize = (double)Resources["PhoneFontSizeSmall"];
        }

        private void Populate(CompoundRoute route)
        {
            Root = new Grid();
            Content = Root;

            Root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
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
                        Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        row += 1;

                        var waitTime = AddTime(leg, row, start: true, arrival: true);

                        showWait = waitTime > TimeSpan.FromMinutes(1);

                        AddDotLine(leg, isFirst, false, true, locationNum, row);
                        locationNum += 1;

                        if (leg.Locs.Length > 0)
                        {
                            var location = leg.Locs[0];
                            AddLocationDetails(location, isFirst, isLast, row);
                        }

                        if (showWait)
                        {
                            Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            row += 1;

                            AddDotLine(leg, false, false, false, null, row);

                            AddWaitDetails(waitTime, row);

                            Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            row += 1;

                            AddTime(leg, row, start: true, arrival: false);

                            AddDotLine(leg, false, false, true, null, row);
                        }
                    }

                    {
                        Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        row += 1;

                        AddDotLine(leg, false, false, false, null, row);

                        AddLegDetails(leg, showWait, row);
                    }

                    if (isLast)
                    {
                        Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        row += 1;

                        AddTime(leg, row, start: false, arrival: true);

                        AddDotLine(leg, false, isLast, true, locationNum, row);
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

        private TimeSpan AddTime(Leg leg, int row, bool start, bool arrival)
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
                        timeString = loc.ArrTime.ToShortTimeString();
                    }
                    else
                    {
                        timeString = loc.DepTime.ToShortTimeString();
                    }
                    waitDuration = loc.DepTime - loc.ArrTime;
                }
                else
                {
                    var loc = leg.Locs[leg.Locs.Length - 1];
                    if (arrival)
                    {
                        timeString = loc.ArrTime.ToShortTimeString();
                    }
                    else
                    {
                        timeString = loc.DepTime.ToShortTimeString();
                    }
                    waitDuration = loc.DepTime - loc.ArrTime;
                }
            }
            var timeTextBlock = new TextBlock
            {
                Text = timeString,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = _normalFontSize,
                Margin = new Thickness(0, 12, 12, 12)
            };
            Grid.SetRow(timeTextBlock, row);
            Grid.SetColumn(timeTextBlock, 0);
            Root.Children.Add(timeTextBlock);

            return waitDuration;
        }

        private void AddDotLine(Leg leg, bool isFirst, bool isLast, bool isLocation, int? number, int row)
        {
            var canvas = new Canvas { Width = 40 };

            Grid.SetRow(canvas, row);
            Grid.SetColumn(canvas, 1);
            Root.Children.Add(canvas);

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
                double diameter = 28;
                if (!number.HasValue)
                {
                    diameter = 14;
                }

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

                if (number.HasValue)
                {
                    var iconControl = new TextBlock
                    {
                        Text = number.Value.ToString(),
                        Foreground = _backgroundBrush,
                        FontFamily = _boldFont,
                        Margin = new Thickness(1.5, 0, 0, 3),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    };
                    Grid.SetRow(iconControl, row);
                    Grid.SetColumn(iconControl, 1);
                    Root.Children.Add(iconControl);
                }
            }
        }

        private void AddLocationDetails(LegLocation location, bool isFirst, bool isLast, int row)
        {
            var locationName = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = _largeFontSize,
                Margin = new Thickness(12, 12, 0, 12)
            };

            if (isFirst || isLast)
            {
                var binding = new Binding(isFirst ? "From" : "To");
                binding.Converter = new DefaultIfNullConverter();
                binding.ConverterParameter = location.Name;
                binding.Source = this;
                BindingOperations.SetBinding(locationName, TextBlock.TextProperty, binding);
            }
            else
            {
                locationName.Text = location.Name;
            }

            Grid.SetRow(locationName, row);
            Grid.SetColumn(locationName, 2);
            Root.Children.Add(locationName);
        }

        private void AddWaitDetails(TimeSpan waitTime, int row)
        {
            string waitText = null;

            if (waitTime.TotalHours < 1)
            {
                waitText = waitTime.ToString(AppResources.RouteControlMinutesWaitFormat);
            }
            else
            {
                waitText = waitTime.ToString(AppResources.RouteControlHoursWaitFormat);
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
            Root.Children.Add(lengthTextBlock);
        }

        private void AddLegDetails(Leg leg, bool hasTime, int row)
        {
            if (leg.Type == "walk")
            {
                var detailsStack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(12, 12, 0, 12)
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
                Root.Children.Add(detailsStack);
            }
            else
            {

                var tilt = new TiltPresenter
                {

                };

                var detailsStack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(12, 12, 0, 12),
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

                };

                for (int i = 1; i < leg.Locs.Length - 1; i += 1)
                {
                    var loc = leg.Locs[i];

                    string stopText = loc.DepTime.ToShortTimeString() + " " + loc.Name;

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
                    Root.Children.Add(tilt);

                    Grid.SetRow(stopsStack, row);
                    Grid.SetColumn(stopsStack, 2);
                    Root.Children.Add(stopsStack);
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
                    Root.Children.Add(allPanel);
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
