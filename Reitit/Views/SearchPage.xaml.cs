using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GalaSoft.MvvmLight.Messaging;

namespace Reitit
{
    public class NewResults
    {
        public object ScrollTo { get; set; }
        public bool HasWritten { get; set; }
    }

    public partial class SearchPage : MapFramePage
    {
        public SearchPage()
        {
            InitializeComponent();
        }

        protected override object ConstructVM(NavigationEventArgs e)
        {
            return new SearchPageVM();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
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
                    SearchResultsSelector.ScrollTo(m.ScrollTo);
                }
                if (!m.HasWritten)
                {
                    Focus();
                }
            });
        }
    }
}