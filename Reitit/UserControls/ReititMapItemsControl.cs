using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Reitit
{
    class ReititMapItemsControl : FrameworkElement, INotifyCollectionChanged
    {
        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(ReititMapItemsControl), new PropertyMetadata(null, ItemsSourceChanged));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ReititMapItemsControl), new PropertyMetadata(null, ItemTemplateChanged));

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }
        public static readonly DependencyProperty ItemTemplateSelectorProperty =
            DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(ReititMapItemsControl), new PropertyMetadata(null, ItemTemplateSelectorChanged));

        public ObservableCollection<FrameworkElement> GeneratedElements { get; private set; }

        public ReititMapItemsControl()
        {
            GeneratedElements = new ObservableCollection<FrameworkElement>();
        }

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ReititMapItemsControl;
            var oldSource = e.OldValue as INotifyCollectionChanged;
            if (oldSource != null)
            {
                oldSource.CollectionChanged -= control.ItemsSource_CollectionChanged;
            }
            control.RegenerateElements();
            var newSource = e.NewValue as INotifyCollectionChanged;
            if (newSource != null)
            {
                newSource.CollectionChanged += control.ItemsSource_CollectionChanged;
            }
        }

        private static void ItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ReititMapItemsControl;
            control.RegenerateElements();
        }

        private static void ItemTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ReititMapItemsControl;
            control.RegenerateElements();
        }

        void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = e.NewItems.Count - 1; i >= 0; --i)
                    {
                        var element = GenerateElement(e.NewItems[i]);
                        GeneratedElements.Insert(e.NewStartingIndex, element);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; ++i)
                    {
                        GeneratedElements.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.OldItems.Count; ++i)
                    {
                        GeneratedElements[e.OldStartingIndex + i] = GenerateElement(e.NewItems[i]);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    var offsets = Enumerable.Range(0, e.NewItems.Count);
                    if (e.NewStartingIndex > e.OldStartingIndex) // Move in reverse order when moving to the right
                    {
                        offsets = offsets.Reverse();
                    }
                    foreach (int i in offsets)
                    {
                        GeneratedElements.Move(e.OldStartingIndex + i, e.NewStartingIndex + i);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    RegenerateElements();
                    break;
            }
        }

        void RegenerateElements()
        {
            GeneratedElements.Clear();
            var items = ItemsSource as IEnumerable;
            if (items != null)
            {
                foreach (var item in items)
                {
                    var element = GenerateElement(item);
                    GeneratedElements.Add(element);
                }
            }
        }

        private FrameworkElement GenerateElement(object item)
        {
            FrameworkElement element;
            var template = GetDataTemplate(item);
            if (template != null)
            {
                element = template.LoadContent() as FrameworkElement;
            }
            else
            {
                element = new ContentControl { Content = item };
            }
            element.DataContext = item;
            return element;
        }

        DataTemplate GetDataTemplate(object item)
        {
            var selector = ItemTemplateSelector;
            if (selector != null)
            {
                return selector.SelectTemplate(item);
            }
            else
            {
                return ItemTemplate;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
