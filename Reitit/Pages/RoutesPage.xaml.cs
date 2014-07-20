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

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RoutesPage : MapContentPage
    {
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
                Panel.SizeChanged += Panel_SizeChanged;
                //double footerOldOffset = Footer.TransformToVisual(Scroll).TransformPoint(new Point(0, 0)).Y;
                //double oldScrollOffset = Scroll.VerticalOffset;

                //await Utils.OnCoreDispatcher(() =>
                //{
                //    double footerNewOffset = Footer.TransformToVisual(Scroll).TransformPoint(new Point(0, 0)).Y;
                //    Scroll.ScrollToVerticalOffset(oldScrollOffset + footerNewOffset - footerOldOffset);
                //    //if (_scrollAnimation != null)
                //    //{
                //    //    _scrollAnimation.To += footerPosition - footerOldPosition;
                //    //}
                //});
            });
            Messenger.Default.Register<ScrollToCurrentMessage>(this, async m =>
            {

                UIElement container = RoutesListView.ContainerFromItem(RoutesListView.SelectedItem) as UIElement;
                double initialItemViewportOffset = container.TransformToVisual(Scroll).TransformPoint(new Point(0, 0)).Y;

                await Utils.OnCoreDispatcher(() =>
                {
                    double to = container.TransformToVisual(Panel).TransformPoint(new Point(0, 0)).Y;
                    to = Math.Max(0, to);
                    to = Math.Min(to, Scroll.ExtentHeight - Scroll.ViewportHeight);

                    double from = to - initialItemViewportOffset;
                    from = Math.Max(0, from);
                    from = Math.Min(from, Scroll.ExtentHeight - Scroll.ViewportHeight);

                    Scroll.ScrollToVerticalOffset(to);
                    ScrollAnimation.From = to - from;
                    ScrollAnimation.To = 0;
                    ScrollBoard.Begin();
                });
            });
        }

        private void Panel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var offset = e.NewSize.Height - e.PreviousSize.Height;
            Utils.OnCoreDispatcher(() =>
            {
                Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset + offset);
            });
            Panel.SizeChanged -= Panel_SizeChanged;
        }
    }
}
