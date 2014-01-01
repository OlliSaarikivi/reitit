using BindableApplicationBar;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Reitit
{
    public abstract class DateTimePickerPopup : PickerPopup<DateTime?> {
        protected LoopingSelector _primarySelectorPart;
        protected LoopingSelector _secondarySelectorPart;
        protected LoopingSelector _tertiarySelectorPart;
        protected void InitializeDateTimePickerPage(LoopingSelector primarySelector, LoopingSelector secondarySelector, LoopingSelector tertiarySelector)
        {
            if (null == primarySelector)
            {
                throw new ArgumentNullException("primarySelector");
            }
            if (null == secondarySelector)
            {
                throw new ArgumentNullException("secondarySelector");
            }
            if (null == tertiarySelector)
            {
                throw new ArgumentNullException("tertiarySelector");
            }

            _primarySelectorPart = primarySelector;
            _secondarySelectorPart = secondarySelector;
            _tertiarySelectorPart = tertiarySelector;

            // Hook up to interesting events
            _primarySelectorPart.DataSource.SelectionChanged += OnDataSourceSelectionChanged;
            _secondarySelectorPart.DataSource.SelectionChanged += OnDataSourceSelectionChanged;
            _tertiarySelectorPart.DataSource.SelectionChanged += OnDataSourceSelectionChanged;
            _primarySelectorPart.IsExpandedChanged += OnSelectorIsExpandedChanged;
            _secondarySelectorPart.IsExpandedChanged += OnSelectorIsExpandedChanged;
            _tertiarySelectorPart.IsExpandedChanged += OnSelectorIsExpandedChanged;

            // Hide all selectors
            _primarySelectorPart.Visibility = Visibility.Collapsed;
            _secondarySelectorPart.Visibility = Visibility.Collapsed;
            _tertiarySelectorPart.Visibility = Visibility.Collapsed;

            // Position and reveal the culture-relevant selectors
            int column = 0;
            foreach (LoopingSelector selector in GetSelectorsOrderedByCulturePattern())
            {
                Grid.SetColumn(selector, column);
                selector.Visibility = Visibility.Visible;
                column++;
            }
        }


        private void OnDataSourceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Push the selected item to all selectors
            DataSource dataSource = (DataSource)sender;
            _primarySelectorPart.DataSource.SelectedItem = dataSource.SelectedItem;
            _secondarySelectorPart.DataSource.SelectedItem = dataSource.SelectedItem;
            _tertiarySelectorPart.DataSource.SelectedItem = dataSource.SelectedItem;
        }

        private void OnSelectorIsExpandedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // Ensure that only one selector is expanded at a time
                _primarySelectorPart.IsExpanded = (sender == _primarySelectorPart);
                _secondarySelectorPart.IsExpanded = (sender == _secondarySelectorPart);
                _tertiarySelectorPart.IsExpanded = (sender == _tertiarySelectorPart);
            }
        }

        protected abstract IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern();

        protected static IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern(string pattern, char[] patternCharacters, LoopingSelector[] selectors)
        {
            if (null == pattern)
            {
                throw new ArgumentNullException("pattern");
            }
            if (null == patternCharacters)
            {
                throw new ArgumentNullException("patternCharacters");
            }
            if (null == selectors)
            {
                throw new ArgumentNullException("selectors");
            }
            if (patternCharacters.Length != selectors.Length)
            {
                throw new ArgumentException("Arrays must contain the same number of elements.");
            }

            // Create a list of index and selector pairs
            List<Tuple<int, LoopingSelector>> pairs = new List<Tuple<int, LoopingSelector>>(patternCharacters.Length);
            for (int i = 0; i < patternCharacters.Length; i++)
            {
                pairs.Add(new Tuple<int, LoopingSelector>(pattern.IndexOf(patternCharacters[i]), selectors[i]));
            }

            // Return the corresponding selectors in order
            return pairs.Where(p => -1 != p.Item1).OrderBy(p => p.Item1).Select(p => p.Item2).Where(s => null != s);
        }

        protected override void InitializeWithCurrent(DateTime? current)
        {
            DateTimeWrapper wrapper = new DateTimeWrapper(current.Value);
            _primarySelectorPart.DataSource.SelectedItem = wrapper;
            _secondarySelectorPart.DataSource.SelectedItem = wrapper;
            _tertiarySelectorPart.DataSource.SelectedItem = wrapper;
            _primarySelectorPart.IsExpanded = false;
            _secondarySelectorPart.IsExpanded = false;
            _tertiarySelectorPart.IsExpanded = false;
        }
    }

    public abstract class LocationPickerPopupBase : PickerPopup<IPickerLocation>
    {
    }

    public abstract class PickerPopup<T> : UserControl
    {
        private Storyboard _openStoryboard, _closeStoryboard;

        private TaskCompletionSource<T> _source;
        private BindableApplicationBar.BindableApplicationBar _oldAppBar;
        private double _oldSTOpacity;
        private bool _oldSTIsVisible;
        private bool _oldIsHitTestVisible;
        private ProgressIndicator _oldSTProgressIndicator;
        private Color _oldSTForegroundColor;
        private Color _oldSTBackgroundColor;

        public static readonly DependencyProperty ApplicationBarProperty = DependencyProperty.Register("ApplicationBar", typeof(BindableApplicationBar.BindableApplicationBar), typeof(PickerPopup<T>), new PropertyMetadata(null));
        public BindableApplicationBar.BindableApplicationBar ApplicationBar
        {
            get { return (BindableApplicationBar.BindableApplicationBar)this.GetValue(ApplicationBarProperty); }
            set { this.SetValue(ApplicationBarProperty, value); }
        }

        public PickerPopup()
        {
            Visibility = Visibility.Collapsed;

            var planeProjection = new PlaneProjection();
            Projection = planeProjection;

            DoubleAnimation openAnimation = new DoubleAnimation();
            openAnimation.From = -50;
            openAnimation.To = 0;
            var openEase = new ExponentialEase();
            openEase.EasingMode = EasingMode.EaseOut;
            openEase.Exponent = 6;
            openAnimation.EasingFunction = openEase;
            openAnimation.Duration = TimeSpan.FromSeconds(0.15);
            Storyboard.SetTargetProperty(openAnimation, new PropertyPath("RotationX"));
            Storyboard.SetTarget(openAnimation, planeProjection);

            _openStoryboard = new Storyboard();
            _openStoryboard.Children.Add(openAnimation);

            DoubleAnimation closeAnimation = new DoubleAnimation();
            closeAnimation.From = 0;
            closeAnimation.To = 30;
            var closeEase = new ExponentialEase();
            closeEase.EasingMode = EasingMode.EaseIn;
            closeEase.Exponent = 6;
            closeAnimation.EasingFunction = closeEase;
            closeAnimation.Duration = TimeSpan.FromSeconds(0.15);
            Storyboard.SetTargetProperty(closeAnimation, new PropertyPath("RotationX"));
            Storyboard.SetTarget(closeAnimation, planeProjection);

            _closeStoryboard = new Storyboard();
            _closeStoryboard.Children.Add(closeAnimation);
            _closeStoryboard.Completed += (s, e) =>
            {
                Visibility = Visibility.Collapsed;
                App.RootFrame.Overlay.Children.Remove(this);
            };
        }

        public async Task<T> Show(T current)
        {
            if (_source != null)
            {
                throw new Exception("Picking already in progress");
            }
            _source = new TaskCompletionSource<T>();

            var page = App.RootFrame.Content as PhoneApplicationPage;
            _oldAppBar = Bindable.GetApplicationBar(page);
            Bindable.SetApplicationBar(page, ApplicationBar);
            _oldIsHitTestVisible = page.IsHitTestVisible;
            page.IsHitTestVisible = false;
            page.BackKeyPress += BackKeyPress;

            _oldSTOpacity = SystemTray.Opacity;
            _oldSTIsVisible = SystemTray.IsVisible;
            _oldSTProgressIndicator = SystemTray.ProgressIndicator;
            _oldSTForegroundColor = SystemTray.ForegroundColor;
            _oldSTBackgroundColor = SystemTray.BackgroundColor;
            SystemTray.Opacity = 0;
            SystemTray.IsVisible = true;
            SystemTray.ProgressIndicator = null;
            SystemTray.ForegroundColor = (Color)Application.Current.Resources["PhoneForegroundColor"];

            InitializeWithCurrent(current);

            AnimateOpen();

            return await _source.Task;
        }

        private void BackKeyPress(object sender, CancelEventArgs e)
        {
            if (_source != null)
            {
                e.Cancel = true;
                Done(default(T));
                var page = App.RootFrame.Content as PhoneApplicationPage;
                page.Focus();
            }
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

        private void Close()
        {
            if (_source == null)
            {
                throw new Exception("Picking not in progress");
            }

            SystemTray.Opacity = _oldSTOpacity;
            SystemTray.IsVisible = _oldSTIsVisible;
            SystemTray.ProgressIndicator = _oldSTProgressIndicator;
            SystemTray.ForegroundColor = _oldSTForegroundColor;
            SystemTray.BackgroundColor = _oldSTBackgroundColor;

            var page = App.RootFrame.Content as PhoneApplicationPage;
            Bindable.SetApplicationBar(page, _oldAppBar);
            page.IsHitTestVisible = _oldIsHitTestVisible;
            page.BackKeyPress -= BackKeyPress;

            AnimateClose();
        }

        private void AnimateClose()
        {
            _openStoryboard.Stop();
            _closeStoryboard.Begin();
        }

        public void Done(T value)
        {
            if (_source != null)
            {
                Close();
                _source.SetResult(value);
                _source = null;
            }
            else
            {
                throw new Exception("Picking not in progress");
            }
        }

        protected void Cancel()
        {
            if (_source != null)
            {
                Close();
                _source.SetCanceled();
                _source = null;
            }
            else
            {
                throw new Exception("Picking not in progress");
            }
        }

        protected abstract void InitializeWithCurrent(T current);
    }
}
