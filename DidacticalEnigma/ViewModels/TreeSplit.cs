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
                children[0].Parent = this;
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
                children[1].Parent = this;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<Element> Children => children;

        private readonly Func<Element> factory;

        private readonly ObservableBatchCollection<Element> children;

        public Split(Func<Element> factory)
        {
            this.factory = factory;
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
        private readonly Action<object> onClose;
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

        private void DoClose()
        {
            switch (Parent)
            {
                case Root root:
                    {
                        // do nothing
                    }
                    break;
                case Split parentSplit:
                    {
                        Element other;
                        if (parentSplit.First == this)
                        {
                            other = parentSplit.Second;
                        }
                        else if (parentSplit.Second == this)
                        {
                            other = parentSplit.First;
                        }
                        else
                        {
                            throw new InvalidOperationException("invariant violated: supposed parents don't have this node as a child");
                        }
                        FixUpGrandParent(parentSplit, other);
                    }
                    break;
                default:
                    throw new InvalidOperationException("invariant violated: leaf node is a parent");
            }

            void FixUpGrandParent(Element parent, Element other)
            {
                switch (parent.Parent)
                {
                    case Root grandParentRoot:
                        {
                            grandParentRoot.Tree = other;
                        }
                        break;
                    case Split grandParentSplit:
                        {
                            if (grandParentSplit.First == parent)
                            {
                                grandParentSplit.First = other;
                            }
                            else if (grandParentSplit.Second == parent)
                            {
                                grandParentSplit.Second = other;
                            }
                            else
                            {
                                throw new InvalidOperationException("invariant violated: supposed parents don't have this node as a child");
                            }
                        }
                        break;
                    default:
                        throw new InvalidOperationException("invariant violated: leaf node is a parent");
                }
            }
        }

        private void Split(Split split)
        {
            var previousParent = Parent;
            split.First = this;
            split.Second = new Leaf(factory, onClose);
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

        public ICommand Close { get; }

        public Leaf(Func<object> factory, Action<object> onClose)
        {
            Content = factory();
            this.factory = factory;
            this.onClose = onClose;

            HSplit = new RelayCommand(() =>
            {
                Split(new HSplit(() => new Leaf(this.factory, this.onClose)));
            });
            VSplit = new RelayCommand(() =>
            {
                Split(new VSplit(() => new Leaf(this.factory, this.onClose)));
            });
            Close = new RelayCommand(() =>
            {
                DoClose();
                this.onClose(Content);
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
        public HSplit(Func<Element> factory) : base(factory)
        {
        }
    }

    public class VSplit : Split
    {
        public VSplit(Func<Element> factory) : base(factory)
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
                children[0].Parent = this;
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
