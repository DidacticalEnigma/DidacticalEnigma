using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DidacticalEnigma.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private static IEnumerable<DependencyObject> GetVisualChildrenRecursively(DependencyObject parent)
        {
            if (parent == null) yield break;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                yield return child;
            }
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                foreach (var d in GetVisualChildrenRecursively(child))
                {
                    yield return d;
                }
            }
        }

        public void InsertTextAtCursor(string text)
        {
            var japaneseTextBox = GetVisualChildrenRecursively(this)
                .OfType<TextBox>()
                .First(t => t.Name == "JapaneseTextInputBox");
            japaneseTextBox.SelectedText = text;
            japaneseTextBox.CaretIndex += japaneseTextBox.SelectedText.Length;
            japaneseTextBox.SelectionLength = 0;
            int lineIndex = japaneseTextBox.GetLineIndexFromCharacterIndex(japaneseTextBox.CaretIndex);
            japaneseTextBox.ScrollToLine(lineIndex);
        }
    }
}
