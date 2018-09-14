using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DidacticalEnigma.ViewModels
{
    public abstract class Element : INotifyPropertyChanged
    {
        private Element parent;

        public Element Parent
        {
            get => parent;
            set
            {
                if (parent == value)
                    return;
                parent = value;
                OnPropertyChanged();
            }
        }

        private GridLength size = new GridLength(1.0, GridUnitType.Star);
        public GridLength Size
        {
            get => size;
            set
            {
                if(value == size)
                    return;

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}