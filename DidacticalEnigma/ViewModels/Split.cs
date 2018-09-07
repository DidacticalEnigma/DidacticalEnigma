using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Utils;

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
}
