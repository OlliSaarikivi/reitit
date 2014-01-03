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
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace Reitit
{
    public partial class LogoFlasher : UserControl
    {
        private Storyboard _fadeStoryboard;

        private static readonly TimeSpan OnTime = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.15);

        public LogoFlasher()
        {
            InitializeComponent();

            if ((Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible)
            {
                Logo.Foreground = (Brush)Application.Current.Resources["PhoneForegroundBrush"];
            }
            else
            {
                Logo.Foreground = (Brush)Application.Current.Resources["PhoneBackgroundBrush"];
            }

            DoubleAnimationUsingKeyFrames fadeAnimation = new DoubleAnimationUsingKeyFrames();
            fadeAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 1,
            });
            fadeAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(OnTime),
                Value = 1,
            });
            var fadeEase = new ExponentialEase();
            fadeEase.EasingMode = EasingMode.EaseIn;
            fadeEase.Exponent = 6;
            fadeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(OnTime + FadeTime),
                Value = 0,
                EasingFunction = fadeEase,
            });
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
            Storyboard.SetTarget(fadeAnimation, this);

            _fadeStoryboard = new Storyboard();
            _fadeStoryboard.Children.Add(fadeAnimation);
            _fadeStoryboard.Completed += (s, e) =>
            {
                App.RootFrame.Overlay.Children.Remove(this);
                App.RootFrame.Navigating -= RootFrame_Navigating;
            };

            App.RootFrame.Overlay.Children.Add(this);
            App.RootFrame.Navigating += RootFrame_Navigating;

            _fadeStoryboard.Begin();
        }

        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_fadeStoryboard.GetCurrentTime() < OnTime)
            {
                _fadeStoryboard.Seek(OnTime);
            }
        }
    }
}
