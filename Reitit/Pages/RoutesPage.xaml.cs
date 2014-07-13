using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

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

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RoutesPage : MapContentPage
    {
        private Storyboard _scrollBoard;
        private DoubleAnimation _scrollAnimation;

        public RoutesPage()
        {
            this.InitializeComponent();
        }

        protected override object ConstructVM(object parameter)
        {
            return new RoutesPageVM((RouteLoader)parameter);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Messenger.Default.Register<KeepScrollMessage>(this, async m =>
            {
                double footerViewportOffset = Footer.TransformToVisual(Scroll).TransformPoint(new Point(0, 0)).Y;
                double footerOldPosition = Footer.TransformToVisual(Panel).TransformPoint(new Point(0, 0)).Y;

                await Utils.OnCoreDispatcher(() =>
                {
                    double footerPosition = Footer.TransformToVisual(Panel).TransformPoint(new Point(0, 0)).Y;
                    Scroll.ScrollToVerticalOffset(footerPosition - footerViewportOffset);
                    if (_scrollAnimation != null)
                    {
                        _scrollAnimation.To += footerPosition - footerOldPosition;
                    }
                });
            });
            Messenger.Default.Register<ScrollToCurrentMessage>(this, async m =>
            {
                UIElement container = RoutesListView.ItemContainerGenerator.ContainerFromItem(RoutesListView.SelectedItem) as UIElement;
                double initialItemViewportOffset = container.TransformToVisual(Scroll).TransformPoint(new Point(0, 0)).Y;

                await Utils.OnCoreDispatcher(() =>
                {
                    if (_scrollBoard != null)
                    {
                        RemoveScrollBoard();
                    }

                    double to = container.TransformToVisual(Panel).TransformPoint(new Point(0, 0)).Y;
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
                    Storyboard.SetTargetProperty(_scrollAnimation, "(ScrollViewerBehavior.VerticalOffset)");
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

        private void Scroll_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (_scrollBoard != null)
            {
                RemoveScrollBoard();
            }
        }
    }
}
