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
using System.Windows.Shapes;

namespace DidacticalEnigma
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        public string AboutText =>
@"This program's goal is to assist in translation.

This program uses and relies on:
- NMeCab project:
    https://osdn.net/projects/nmecab
- JMdict project:
    http://www.edrdg.org/wiki/index.php/JMdict-EDICT_Dictionary_Project
- KRADFILE/RADKFILE:
    http://www.edrdg.org/krad/kradinf.html
- Resources available from the following wikipedia pages:
    https://en.wiktionary.org/wiki/Appendix:Easily_confused_Japanese_kana
    https://en.wikipedia.org/wiki/Hiragana
    https://en.wikipedia.org/wiki/Katakana
";

        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
