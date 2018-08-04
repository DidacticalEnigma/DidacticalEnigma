using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DidacticalEnigma.Utils
{
    public class GridEx : Grid
    {
        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Rows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(int), typeof(GridEx), new PropertyMetadata(1, OnRowsChanged));

        private static void OnRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (GridEx)d;
            var oldValue = (int)e.OldValue;
            var newValue = (int)e.NewValue;
            self.RowDefinitions.Clear();
            for (int i = 0; i < newValue; i++)
                self.RowDefinitions.Add(new RowDefinition());
        }

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Columns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(GridEx), new PropertyMetadata(1, OnColumnsChanged));

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (GridEx)d;
            var oldValue = (int)e.OldValue;
            var newValue = (int)e.NewValue;
           self.ColumnDefinitions.Clear();
            for (int i = 0; i < newValue; i++)
                self.ColumnDefinitions.Add(new ColumnDefinition());
        }
    }
}
