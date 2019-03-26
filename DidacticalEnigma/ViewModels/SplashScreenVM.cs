using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DidacticalEnigma.ViewModels
{
    class SplashScreenVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string progressReport = "";
        public string ProgressReport
        {
            get => progressReport;
            set
            {
                if (progressReport == value)
                    return;
                progressReport = value;
                OnPropertyChanged();
            }
        }
    }
}
