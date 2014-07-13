using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Reitit
{
    public class NewResults
    {
        public object ScrollTo { get; set; }
        public bool HasWritten { get; set; }
    }

    public sealed partial class SearchPage : MapContentPage
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        protected override object ConstructVM(object parameter)
        {
            return new SearchPageVM();
        }

        private void SearchBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            var binding = (sender as TextBox).GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Messenger.Default.Register<NewResults>(this, m =>
            {
                UpdateLayout();
                if (m.ScrollTo != null)
                {
                    SearchResultsList.ScrollIntoView(m.ScrollTo);
                }
                if (!m.HasWritten)
                {
                    Focus(FocusState.Programmatic);
                }
            });
        }
    }
}
