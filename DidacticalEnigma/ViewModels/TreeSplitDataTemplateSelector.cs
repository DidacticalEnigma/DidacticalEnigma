using System.Windows;
using System.Windows.Controls;

namespace DidacticalEnigma.ViewModels
{
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