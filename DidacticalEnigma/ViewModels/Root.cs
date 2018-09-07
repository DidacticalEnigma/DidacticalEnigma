using System;
using System.Collections.Generic;
using System.ComponentModel;
using DidacticalEnigma.Core.Utils;

namespace DidacticalEnigma.ViewModels
{
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
}