using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.ViewManagement;
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
    public abstract class PageBase : Page
    {
        public NavigationHelper NavigationHelper { get; private set; }

        protected object NavigationParameter { get; private set; }

        protected List<Tombstoner> Tombstoners
        {
            get
            {
                return _tombstoners;
            }
        }
        private List<Tombstoner> _tombstoners = new List<Tombstoner>();

        public PageBase()
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            NavigationHelper = ConstructNavigation();
            NavigationHelper.LoadState += this.NavigationHelper_LoadState;
            NavigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            OnBackPressed(e);
        }

        protected virtual void OnBackPressed(BackPressedEventArgs e) { }

        protected virtual NavigationHelper ConstructNavigation()
        {
            return new NavigationHelper(this);
        }

        protected abstract object ConstructVM(object parameter);

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (e.PageState != null)
            {
                DataContext = e.PageState["DataContext"];
                foreach (var stoner in Tombstoners)
                {
                    stoner.RestoreFrom(e.PageState);
                }
                LoadState(e.PageState);
            }
            else
            {
                DataContext = ConstructVM(NavigationParameter);
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["DataContext"] = DataContext;
            foreach (var stoner in Tombstoners)
            {
                stoner.TombstoneTo(e.PageState);
            }
            SaveState(e.PageState);
        }

        protected virtual void LoadState(IDictionary<string, object> state) { }
        protected virtual void SaveState(IDictionary<string, object> state) { }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int)
            {
                object _navigationParameter;
                App.Current.ParamCache.TryGetParam((int)e.Parameter, out _navigationParameter);
                NavigationParameter = _navigationParameter;
            }
            NavigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedFrom(e);
            Messenger.Default.Unregister(this);
        }
    }
}
