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
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace Reitit
{
    public class ScrollToCurrentMessage { }

    public class KeepScrollMessage { }

    public class ScrollViewerBehavior
    {
        public static DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset",
                                                typeof(double),
                                                typeof(ScrollViewerBehavior),
                                                new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }
        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }
        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = target as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }

    public partial class RoutesPage : MapFramePage
    {
        private Storyboard _scrollBoard;
        private DoubleAnimation _scrollAnimation;

        public RoutesPage()
        {
            InitializeComponent();
        }

        protected override object ConstructVM(NavigationEventArgs e)
        {
            var loader = (RouteLoader)App.Current.Parameters.RemoveParam(NavigationContext.QueryString, "loader");
            return new RoutesPageVM(loader);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Messenger.Default.Register<KeepScrollMessage>(this, m =>
            {
                double footerViewportOffset = Footer.TransformToVisual(Scroll).Transform(new Point(0, 0)).Y;
                double footerOldPosition = Footer.TransformToVisual(Panel).Transform(new Point(0, 0)).Y;

                Dispatcher.BeginInvoke(() =>
                {
                    double footerPosition = Footer.TransformToVisual(Panel).Transform(new Point(0, 0)).Y;
                    Scroll.ScrollToVerticalOffset(footerPosition - footerViewportOffset);
                    if (_scrollAnimation != null)
                    {
                        _scrollAnimation.To += footerPosition - footerOldPosition;
                    }
                });
            });
            Messenger.Default.Register<ScrollToCurrentMessage>(this, m =>
            {
                UIElement container = RoutesListBox.ItemContainerGenerator.ContainerFromItem(RoutesListBox.SelectedItem) as UIElement;
                double initialItemViewportOffset = container.TransformToVisual(Scroll).Transform(new Point(0, 0)).Y;

                Dispatcher.BeginInvoke(() =>
                {
                    if (_scrollBoard != null)
                    {
                        RemoveScrollBoard();
                    }

                    double to = container.TransformToVisual(Panel).Transform(new Point(0, 0)).Y;
                    to = Math.Max(0, to);
                    to = Math.Min(to, Scroll.ExtentHeight - Scroll.ViewportHeight);

                    double from = to - initialItemViewportOffset;
                    from = Math.Max(0, from);
                    from = Math.Min(from, Scroll.ExtentHeight - Scroll.ViewportHeight);
                    Scroll.ScrollToVerticalOffset(from);

                    var ease = new ExponentialEase();
                    ease.EasingMode = EasingMode.EaseOut;
                    ease.Exponent = 3;

                    _scrollAnimation = new DoubleAnimation();
                    _scrollAnimation.To = to;
                    _scrollAnimation.EasingFunction = ease;
                    _scrollAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.15));
                    _scrollBoard = new Storyboard();
                    _scrollBoard.Children.Add(_scrollAnimation);
                    Storyboard.SetTarget(_scrollAnimation, Scroll);
                    Storyboard.SetTargetProperty(_scrollAnimation, new PropertyPath(ScrollViewerBehavior.VerticalOffsetProperty));
                    _scrollBoard.Completed += (s, e2) => RemoveScrollBoard();
                    _scrollBoard.Begin();
                });
            });
        }

        private void RemoveScrollBoard()
        {
            ScrollViewerBehavior.SetVerticalOffset(Scroll, ScrollViewerBehavior.GetVerticalOffset(Scroll));
            _scrollBoard.Stop();
            _scrollBoard = null;
            _scrollAnimation = null;
        }

        private void Scroll_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            if (_scrollBoard != null)
            {
                RemoveScrollBoard();
            }
        }
    }
}