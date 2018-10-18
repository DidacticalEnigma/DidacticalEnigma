using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using Newtonsoft.Json;

namespace DidacticalEnigma.ViewModels
{
    public abstract class Element : INotifyPropertyChanged
    {
        private Element parent;

        [JsonIgnore]
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

        [JsonProperty]
        protected abstract string Type { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}