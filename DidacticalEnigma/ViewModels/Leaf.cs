﻿using System;
using System.Windows.Input;

namespace DidacticalEnigma.ViewModels
{
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
}