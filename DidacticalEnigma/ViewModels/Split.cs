using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Utility.Utils;

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

        private GridLength leftDimension = new GridLength(1, GridUnitType.Star);
        public GridLength LeftDimension
        {
            get => leftDimension;
            set
            {
                if (value == leftDimension)
                    return;

                leftDimension = value;
                OnPropertyChanged();
            }
        }

        private GridLength rightDimension = new GridLength(1, GridUnitType.Star);
        public GridLength RightDimension
        {
            get => rightDimension;
            set
            {
                if (value == rightDimension)
                    return;

                rightDimension = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public IReadOnlyList<Element> Children => children;

        private readonly ObservableBatchCollection<Element> children;

        protected Split()
        {
            children = new ObservableBatchCollection<Element>
            {
                null,
                null
            };
        }
    }
}
