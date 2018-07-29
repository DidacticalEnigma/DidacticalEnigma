using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace DidacticalEnigma
{
    public class SimpleFlowDocument : FlowDocument
    {
        private Block rootElement;

        public Type RootTextElementType
        {
            get { return (Type)GetValue(RootTextElementTypeProperty); }
            set { SetValue(RootTextElementTypeProperty, value); }
        }

        public static readonly DependencyProperty RootTextElementTypeProperty =
            DependencyProperty.Register(nameof(RootTextElementType), typeof(Type), typeof(SimpleFlowDocument), new PropertyMetadata(typeof(Section), OnRootTextElementTypeChanged));

        private static void OnRootTextElementTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (SimpleFlowDocument)d;
            self.OnDataSourceCollectionChanged(self, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register(nameof(DataSource), typeof(IEnumerable), typeof(SimpleFlowDocument), new PropertyMetadata(null, OnDataSourceChanged));

        public DataTemplateSelector ContentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ContentTemplateSelectorProperty); }
            set { SetValue(ContentTemplateSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentTemplateSelectorProperty =
            DependencyProperty.Register(nameof(ContentTemplateSelector), typeof(DataTemplateSelector), typeof(SimpleFlowDocument), new PropertyMetadata(null, OnContentTemplateSelectorChanged));

        private static void OnContentTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (SimpleFlowDocument)d;
            self.OnDataSourceCollectionChanged(self, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (SimpleFlowDocument)d;
            var oldCollection = (IEnumerable)e.OldValue;
            var newCollection = (IEnumerable)e.NewValue;
            {
                if (oldCollection is INotifyCollectionChanged n)
                {
                    n.CollectionChanged -= self.OnDataSourceCollectionChanged;
                }
            }
            {
                if (newCollection is INotifyCollectionChanged n)
                {
                    n.CollectionChanged += self.OnDataSourceCollectionChanged;
                }
            }
            self.OnDataSourceCollectionChanged(self.DataSource, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnDataSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: handle it
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    {
                        rootElement = (Block)(Activator.CreateInstance(RootTextElementType));
                        IAddChild selfAdd = rootElement;
                        foreach (var o in DataSource ?? Enumerable.Empty<object>())
                        {
                            TextElement c = Create(o);
                            selfAdd.AddChild(c);
                            c.DataContext = o;

                            // GROSS HACK
                            {
                                if (c.ToolTip is FrameworkElement tooltip)
                                {
                                    tooltip.DataContext = o;
                                }
                            }
                        }
                        this.Blocks.Clear();
                        this.Blocks.Add((Block)selfAdd);

                    }
                    break;
            }

            TextElement Create(object o)
            {
                var dataTemplate = ContentTemplateSelector.SelectTemplate(o, this);
                var c = (TextElement)dataTemplate.LoadContent();
                return c;
            }
        }
    }

    public class TrivialTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Default { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return Default;
        }
    }
}
