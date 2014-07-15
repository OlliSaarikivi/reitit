using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Reitit
{
    public class ScrollIntoViewMessage
    {
        public object Item { get; set; }
    }
    public class UnfocusTextBoxMessage { }
    public class SelectionChangedMessage { }

    public sealed partial class LocationPicker : UserControl
    {
        private AppBar _oldBar;
        private Color? _oldStatusBarColor;
        private CommandBar _commandBar;
        private AppBarButton _acceptButton;
        private AppBarButton _cancelButton;

        public IPickerLocation Value
        {
            get { return (IPickerLocation)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(IPickerLocation), typeof(LocationPicker), new PropertyMetadata(null, (s, e) =>
            {
                var location = (IPickerLocation)e.NewValue;
                ((LocationPicker)s).LocationText.Text = location.Name;
            }));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(LocationPicker), new PropertyMetadata(null));

        public LocationPicker()
        {
            this.InitializeComponent();

            _commandBar = new CommandBar();
            _acceptButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = Utils.GetString("PickerDone"),
            };
            _commandBar.PrimaryCommands.Add(_acceptButton);
            _cancelButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Cancel),
                Label = Utils.GetString("PickerCancel"),
            };
            _commandBar.PrimaryCommands.Add(_cancelButton);
        }

        LocationPickerFlyoutVM _vm;
        private void PickerFlyout_Opening(object sender, object e)
        {
            _vm = new LocationPickerFlyoutVM();
            _acceptButton.Command = new RelayCommand(() =>
            {
                Value = _vm.SelectedResult;
                Flyout.Hide();
            });
            _acceptButton.IsEnabled = false;
            _cancelButton.Command = new RelayCommand(() =>
            {
                Flyout.Hide();
            });
            FlyoutRoot.DataContext = _vm;

            var frame = (Frame)Window.Current.Content;
            var page = (Page)frame.Content;
            _oldBar = page.BottomAppBar;
            page.BottomAppBar = _commandBar;

            var statusBar = StatusBar.GetForCurrentView();
            _oldStatusBarColor = statusBar.ForegroundColor;
            statusBar.ForegroundColor = (Color)App.Current.Resources["PhoneForegroundColor"];

            Messenger.Default.Register<ScrollIntoViewMessage>(this, m =>
            {
                LocationsListView.ScrollIntoView(m.Item, ScrollIntoViewAlignment.Default);
            });
            Messenger.Default.Register<UnfocusTextBoxMessage>(this, m =>
            {
                this.Focus(FocusState.Programmatic);
            });
        }

        private void PickerFlyout_Closed(object sender, object e)
        {
            var frame = (Frame)Window.Current.Content;
            var page = (Page)frame.Content;
            page.BottomAppBar = _oldBar;
            _oldBar = null;

            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ForegroundColor = _oldStatusBarColor;

            Messenger.Default.Unregister(this);

            //Utils.OnCoreDispatcher(() =>
            //{
            //    FlyoutRoot.DataContext = null;
            //    _acceptButton.Command = null;
            //    _cancelButton.Command = null;
            //});
        }

        private void LocationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _acceptButton.IsEnabled = _vm.SelectedResult != null;
        }
    }
}
