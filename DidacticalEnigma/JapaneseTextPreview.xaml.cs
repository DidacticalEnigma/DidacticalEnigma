using DidacticalEnigma.Models;
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

        public IEnumerable<LineVM> Lines
        {
            get { return (IEnumerable<LineVM>)GetValue(LinesProperty); }
            set { SetValue(LinesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Lines.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinesProperty =
            DependencyProperty.Register(
                nameof(Lines),
                typeof(IEnumerable<LineVM>),
                typeof(JapaneseTextPreview),
                new PropertyMetadata(null));

        public CodePointVM SelectedCharacter
        {
            get { return (CodePointVM)GetValue(SelectedCharacterProperty); }
            set { SetValue(SelectedCharacterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Lines.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCharacterProperty =
            DependencyProperty.Register(
                nameof(SelectedCharacter),
                typeof(CodePointVM),
                typeof(JapaneseTextPreview),
                new PropertyMetadata(null));

        public WordVM SelectedWord
        {
            get { return (WordVM)GetValue(SelectedWordProperty); }
            set { SetValue(SelectedWordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Lines.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedWordProperty =
            DependencyProperty.Register(
                nameof(SelectedWord),
                typeof(WordVM),
                typeof(JapaneseTextPreview),
                new PropertyMetadata(null));

        private StackPanel previousClickedWord = null;

        private TextBlock previousClickedLetter = null;

        private void SelectText(object sender, MouseEventArgs e)
        {
            if (!(e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
                return;

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
                    SetCurrentValue(SelectedWordProperty, (WordVM)clickedWordPanel.DataContext);
                }
                previousClickedLetter = clickedLetter;
                SetCurrentValue(SelectedCharacterProperty, (CodePointVM)clickedLetter.DataContext);
            }
            //clickedItem.SetCurrentValue(BackgroundProperty, Brushes.Yellow);
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
