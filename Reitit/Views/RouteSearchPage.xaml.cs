using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Media.Animation;

namespace Reitit
{
    public class AnimateAdvanced { }

    public partial class RouteSearchPage : MapFramePage
    {
        public RouteSearchPage()
        {
            InitializeComponent();
            Tombstoners.Add(new ScrollViewerTombstoner(ContentScroll));
        }

        protected override object ConstructVM(NavigationEventArgs e)
        {
            return new RouteSearchPageVM();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //Messenger.Default.Register<AnimateAdvanced>(this, m =>
            //{
            //    AdvancedPanel.Opacity = 0;
            //    Dispatcher.BeginInvoke(() =>
            //    {
            //        var ease = new ExponentialEase();
            //        ease.EasingMode = EasingMode.EaseOut;
            //        ease.Exponent = 3;

            //        DoubleAnimation fadeInAnimation = new DoubleAnimation();
            //        fadeInAnimation.To = 1;
            //        fadeInAnimation.EasingFunction = ease;
            //        fadeInAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            //        Storyboard storyboard = new Storyboard();
            //        storyboard.Children.Add(fadeInAnimation);
            //        Storyboard.SetTarget(fadeInAnimation, AdvancedPanel);
            //        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));
            //        storyboard.Begin();
            //    });
            //});
        }

        private void ListPicker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((ListPicker)sender).KeepExpandedInView(ContentScroll);
        }

        private void MarginSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MarginSlider.Value = (int)MarginSlider.Value;
        }
    }
}