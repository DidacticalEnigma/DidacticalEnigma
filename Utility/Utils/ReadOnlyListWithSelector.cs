using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Utility.Utils
{
    public class ReadOnlyListWithSelector<T> : IReadOnlyList<T>, INotifyPropertyChanged
    {
        private readonly IReadOnlyList<T> underlying;

        private int selectedIndex = -1;
        private readonly bool allowDeselect;

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value)
                    return;

                if(!(-1 <= value && value < this.Count))
                    throw new ArgumentOutOfRangeException();
                if(!allowDeselect && value == -1)
                    throw new ArgumentOutOfRangeException();
                selectedIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
            }
        }

        public T SelectedItem
        {
            get => selectedIndex == -1 ? default(T) : this[selectedIndex];
        }

        public ReadOnlyListWithSelector(IReadOnlyList<T> readOnlyList = null, bool allowDeselect = true)
        {
            this.allowDeselect = allowDeselect;
            this.underlying = readOnlyList ?? Array.Empty<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return underlying.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => underlying.Count;

        public T this[int index] => underlying[index];

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
