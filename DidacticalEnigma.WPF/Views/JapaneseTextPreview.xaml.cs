using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DidacticalEnigma.ViewModels;

namespace DidacticalEnigma.Views
{
    /// <summary>
    /// Interaction logic for JapaneseTextPreview.xaml
    /// </summary>
    public partial class JapaneseTextPreview : UserControl
    {
        public JapaneseTextPreview()
        {
            InitializeComponent();
        }

        public IReadOnlyList<LineVM> Lines
        {
            get { return (IReadOnlyList<LineVM>)GetValue(LinesProperty); }
            set { SetValue(LinesProperty, value); }
        }

        /// <summary>Identifies the <see cref="Lines"/> dependency property.</summary>
        public static readonly DependencyProperty LinesProperty =
            DependencyProperty.Register(
                nameof(Lines),
                typeof(IReadOnlyList<LineVM>),
                typeof(JapaneseTextPreview),
                new PropertyMetadata(null, OnLinesChanged));

        private static void OnLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (JapaneseTextPreview)d;
            var newValue = (IReadOnlyList<LineVM>)e.NewValue;
            self.lines = newValue;
        }

        public SelectionInfoVM SelectionInfo
        {
            get { return (SelectionInfoVM)GetValue(SelectionInfoProperty); }
            set { SetValue(SelectionInfoProperty, value); }
        }

        /// <summary>Identifies the <see cref="SelectionInfo"/> dependency property.</summary>
        public static readonly DependencyProperty SelectionInfoProperty =
            DependencyProperty.Register(
                nameof(SelectionInfo),
                typeof(SelectionInfoVM),
                typeof(JapaneseTextPreview),
                new PropertyMetadata(null));

        private IReadOnlyList<LineVM> lines;

        private StackPanel previousClickedWord = null;

        private TextBlock previousClickedLetter = null;

        private void SelectText(object sender, MouseEventArgs e)
        {
            var shiftWasPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (!(e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
                return;

            CodePointVM codePointVM = null;
            WordVM wordVM = null;
            string text = string.Join("", Lines.SelectMany(line => line.Words.Select(word => word.StringForm)));

            var clickedPoint = e.GetPosition(null);
            var clickedLetter =
                FindAncestor<TextBlock>((DependencyObject)e.OriginalSource);
            if (clickedLetter == null)
                return;
            var clickedWordPanel = FindAncestor<StackPanel>(clickedLetter);
            var resources = Application.Current.Resources;
            if (clickedLetter != previousClickedLetter && clickedLetter.Text.Trim() != "")
            {
                if (previousClickedLetter != null)
                {
                    previousClickedLetter.Background = Brushes.Transparent;
                }
                clickedLetter.Background = (Brush)resources["SelectedLetterBrush"];
                if(clickedWordPanel != previousClickedWord)
                {
                    if(previousClickedWord != null)
                    {
                        previousClickedWord.Background = Brushes.Transparent;
                    }
                    clickedWordPanel.Background = (Brush)resources["SelectedWordBrush"];
                    previousClickedWord = clickedWordPanel;
                    wordVM = (WordVM)clickedWordPanel.DataContext;
                }
                previousClickedLetter = clickedLetter;
                codePointVM = (CodePointVM)clickedLetter.DataContext;
                SetCurrentValue(SelectionInfoProperty, SelectionInfo == null
                    ? new SelectionInfoVM(codePointVM, wordVM, AllText)
                    : SelectionInfo.Clone(codePointVM, wordVM));
            }
        }

        private string AllText()
        {
            return string.Join("\n", lines.Select(l => string.Join(" ", l.Words.Select(w => w.StringForm))));
        }

        private static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            if(current == null)
            {
                return null;
            }
            do
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }
    }
}
