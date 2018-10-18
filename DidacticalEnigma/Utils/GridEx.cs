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

        /// <summary>Identifies the <see cref="Rows"/> dependency property.</summary>
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(nameof(Rows), typeof(int), typeof(GridEx), new PropertyMetadata(1, OnRowsChanged));

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

        /// <summary>Identifies the <see cref="Columns"/> dependency property.</summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(int), typeof(GridEx), new PropertyMetadata(1, OnColumnsChanged));

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
