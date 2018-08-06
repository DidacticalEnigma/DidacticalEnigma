using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DidacticalEnigma.ViewModels
{
    public abstract class Split : Element
    {
        public Element First
        {
            get => children[0];
            set
            {
                if (children[0] == value)
                    return;
                children[0] = value;
                OnPropertyChanged();
            }
        }

        public Element Second
        {
            get => children[1];
            set
            {
                if (children[1] == value)
                    return;
                children[1] = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<Element> Children => children;

        private readonly Func<Element> factory;

        private readonly ObservableBatchCollection<Element> children;

        public Split(Func<Element> factory, Element parent)
        {
            this.factory = factory;
            Parent = parent;
            children = new ObservableBatchCollection<Element>
            {
                null,
                null
            };
        }
    }

    public class Leaf : Element
    {
        private readonly Func<object> factory;
        private object content;

        public object Content
        {
            get => content;
            set
            {
                if (object.Equals(content, value))
                    return;
                content = value;
                OnPropertyChanged();
            }
        }

        private void Split(Split split)
        {
            var previousParent = Parent;
            split.First = this;
            split.First.Parent = split;
            split.Second = new Leaf(factory);
            split.Second.Parent = split;
            switch (previousParent)
            {
                case Root root:
                    {
                        root.Tree = split;
                    }
                    break;
                case Split parentSplit:
                    {
                        if (parentSplit.First == this)
                        {
                            parentSplit.First = split;
                        }
                        else if(parentSplit.Second == this)
                        {
                            parentSplit.Second = split;
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException("invariant violated: leaf node is a parent");
            }
        }

        public ICommand HSplit { get; }

        public ICommand VSplit { get; }

        public Leaf(Func<object> factory)
        {
            Content = factory();
            this.factory = factory;

            HSplit = new RelayCommand(() =>
            {
                Split(new HSplit(() => new Leaf(this.factory), Parent));
            });
            VSplit = new RelayCommand(() =>
            {
                Split(new VSplit(() => new Leaf(factory), Parent));
            });
        }
    }

    public abstract class Element : INotifyPropertyChanged
    {
        private Element parent;

        public Element Parent
        {
            get => parent;
            set
            {
                if (parent == value)
                    return;
                parent = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HSplit : Split
    {
        public HSplit(Func<Element> factory, Element parent) : base(factory, parent)
        {
        }
    }

    public class VSplit : Split
    {
        public VSplit(Func<Element> factory, Element parent) : base(factory, parent)
        {
        }
    }

    public class Root : Element, INotifyPropertyChanged
    {
        private ObservableBatchCollection<Element> children;

        public Element Tree
        {
            get => children[0];
            set
            {
                if (children[0] == value)
                    return;
                children[0] = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<Element> Children => children;

        public Root(Func<Element> factory)
        {
            children = new ObservableBatchCollection<Element>
            {
                null
            };
            Tree = factory();
            Tree.Parent = this;
        }
    }

    public class TreeSplitDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RootTemplate { get; set; }

        public DataTemplate HSplitTemplate { get; set; }

        public DataTemplate VSplitTemplate { get; set; }

        public DataTemplate LeafTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Root)
                return RootTemplate;
            if (item is HSplit)
                return HSplitTemplate;
            if (item is VSplit)
                return VSplitTemplate;
            if (item is Leaf)
                return LeafTemplate;
            return null;
        }
    }
}
