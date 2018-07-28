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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateOnEnter(object sender, KeyEventArgs e)
        {
            var bindingExpression = BindingOperations.GetBindingExpression((TextBox)sender, TextBox.TextProperty);
            if (e.Key == Key.Enter)
            {
                bindingExpression?.UpdateSource();
            }
            if (e.Key == Key.Escape)
            {
                bindingExpression?.UpdateTarget();
            }
        }
    }
}
