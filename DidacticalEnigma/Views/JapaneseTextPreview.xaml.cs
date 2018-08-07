using DidacticalEnigma.Models;
using DidacticalEnigma.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DidacticalEnigma
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
                new PropertyMetadata(null));

        public SelectionInfoVM SelectionInfo
        {
            get { return (SelectionInfoVM)GetValue(SelectionInfoProperty); }
            set { SetValue(SelectionInfoProperty, value); }
        }

        /// <summary>Identifies the <see cref="SelectedCharacter"/> dependency property.</summary>
        public static readonly DependencyProperty SelectionInfoProperty =
            DependencyProperty.Register(
                nameof(SelectionInfo),
                typeof(SelectionInfoVM),
                typeof(JapaneseTextPreview),
                new PropertyMetadata(null));


        private StackPanel previousClickedWord = null;

        private TextBlock previousClickedLetter = null;

        private void SelectText(object sender, MouseEventArgs e)
        {
            var shiftWasPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (!(e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
                return;

            CodePointVM codePointVM = null;
            WordVM wordVM = null;
            string text = null;

            var clickedPoint = e.GetPosition(null);
            var clickedLetter =
                FindAncestor<TextBlock>((DependencyObject)e.OriginalSource);
            if (clickedLetter == null)
                return;
            var clickedWordPanel = FindAncestor<StackPanel>(clickedLetter);
            if (clickedLetter != previousClickedLetter && clickedLetter.Text.Trim() != "")
            {
                if (previousClickedLetter != null)
                {
                    previousClickedLetter.Background = Brushes.Transparent;
                }
                clickedLetter.Background = Brushes.Yellow;
                if(clickedWordPanel != previousClickedWord)
                {
                    if(previousClickedWord != null)
                    {
                        previousClickedWord.Background = Brushes.Transparent;
                    }
                    clickedWordPanel.Background = Brushes.AntiqueWhite;
                    previousClickedWord = clickedWordPanel;
                    wordVM = (WordVM)clickedWordPanel.DataContext;
                }
                previousClickedLetter = clickedLetter;
                codePointVM = (CodePointVM)clickedLetter.DataContext;
                SetCurrentValue(SelectionInfoProperty, SelectionInfo == null
                    ? new SelectionInfoVM(codePointVM, wordVM, text)
                    : SelectionInfo.Clone(codePointVM, wordVM, text));
            }
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
                var ancestor = current as T;
                if (ancestor != null)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }
    }
}
