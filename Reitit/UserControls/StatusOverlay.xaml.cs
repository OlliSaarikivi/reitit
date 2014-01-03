using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Animation;
using System.Windows.Media;
using BindableApplicationBar;
using System.ComponentModel;

namespace Reitit
{
    public partial class StatusOverlay : UserControl
    {
        private Storyboard _openStoryboard, _closeStoryboard;
        private bool _shown = false;
        private BindableApplicationBar.BindableApplicationBar _oldAppBar;
        private double _oldSTOpacity;
        private bool _oldSTIsVisible;
        private bool _oldIsHitTestVisible;
        private ProgressIndicator _oldSTProgressIndicator;
        private Color _oldSTForegroundColor;
        private Color _oldSTBackgroundColor;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(StatusOverlay), new PropertyMetadata(null, TextChanged));

        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var overlay = d as StatusOverlay;
            if (overlay != null)
            {
                overlay.Status.Text = e.NewValue as string;
            }
        }

        public StatusOverlay()
        {
            InitializeComponent();

            App.RootFrame.Navigating += (s, e) =>
            {
                if (_shown && e.NavigationMode != NavigationMode.Back)
                {
                    Hide(false);
                }
            };

            DoubleAnimation openAnimation = new DoubleAnimation();
            openAnimation.From = 0;
            openAnimation.To = 1;
            var openEase = new ExponentialEase();
            openEase.EasingMode = EasingMode.EaseOut;
            openEase.Exponent = 6;
            openAnimation.EasingFunction = openEase;
            openAnimation.Duration = TimeSpan.FromSeconds(0.15);
            Storyboard.SetTargetProperty(openAnimation, new PropertyPath("Opacity"));
            Storyboard.SetTarget(openAnimation, Root);

            _openStoryboard = new Storyboard();
            _openStoryboard.Children.Add(openAnimation);

            DoubleAnimation closeAnimation = new DoubleAnimation();
            closeAnimation.From = 1;
            closeAnimation.To = 0;
            var closeEase = new ExponentialEase();
            closeEase.EasingMode = EasingMode.EaseIn;
            closeEase.Exponent = 6;
            closeAnimation.EasingFunction = closeEase;
            closeAnimation.Duration = TimeSpan.FromSeconds(0.15);
            Storyboard.SetTargetProperty(closeAnimation, new PropertyPath("Opacity"));
            Storyboard.SetTarget(closeAnimation, Root);

            _closeStoryboard = new Storyboard();
            _closeStoryboard.Children.Add(closeAnimation);
            _closeStoryboard.Completed += (s, e) =>
            {
                Visibility = Visibility.Collapsed;
                App.RootFrame.Overlay.Children.Remove(this);
            };
        }

        public void Show()
        {
            if (_shown)
            {
                throw new Exception("Already shown");
            }

            var page = App.RootFrame.Content as PhoneApplicationPage;
            _oldAppBar = Bindable.GetApplicationBar(page);
            Bindable.SetApplicationBar(page, null);
            _oldIsHitTestVisible = page.IsHitTestVisible;
            page.IsHitTestVisible = false;

            _oldSTOpacity = SystemTray.Opacity;
            _oldSTIsVisible = SystemTray.IsVisible;
            _oldSTProgressIndicator = SystemTray.ProgressIndicator;
            _oldSTForegroundColor = SystemTray.ForegroundColor;
            _oldSTBackgroundColor = SystemTray.BackgroundColor;
            SystemTray.Opacity = 0;
            SystemTray.IsVisible = true;
            SystemTray.ProgressIndicator = null;
            SystemTray.ForegroundColor = (Color)Application.Current.Resources["PhoneForegroundColor"];

            AnimateOpen();
        }

        private void AnimateOpen()
        {
            _closeStoryboard.Stop();
            if (!App.RootFrame.Overlay.Children.Contains(this))
            {
                App.RootFrame.Overlay.Children.Add(this);
            }
            Visibility = Visibility.Visible;
            _openStoryboard.Begin();
        }

        private void Hide(bool animate = true)
        {
            if (!_shown)
            {
                throw new Exception("Not shown");
            }
            _shown = false;

            SystemTray.Opacity = _oldSTOpacity;
            SystemTray.IsVisible = _oldSTIsVisible;
            SystemTray.ProgressIndicator = _oldSTProgressIndicator;
            SystemTray.ForegroundColor = _oldSTForegroundColor;
            SystemTray.BackgroundColor = _oldSTBackgroundColor;

            var page = App.RootFrame.Content as PhoneApplicationPage;
            Bindable.SetApplicationBar(page, _oldAppBar);
            page.IsHitTestVisible = _oldIsHitTestVisible;

            AnimateClose(animate);
        }

        private void AnimateClose(bool animate)
        {
            _openStoryboard.Stop();
            if (animate)
            {
                _closeStoryboard.Begin();
            }
            else
            {
                Visibility = Visibility.Collapsed;
                App.RootFrame.Overlay.Children.Remove(this);
            }
        }
    }
}
