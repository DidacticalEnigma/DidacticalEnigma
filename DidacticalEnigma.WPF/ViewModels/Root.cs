using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{
    public class Root : Element
    {
        private readonly ObservableBatchCollection<Element> children;

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

        [JsonIgnore]
        public IReadOnlyList<Element> Children => children;

        public Root(Func<Element> factory)
        {
            children = new ObservableBatchCollection<Element>
            {
                null
            };
            Tree = factory();
        }

        protected override string Type => "root";
    }
}