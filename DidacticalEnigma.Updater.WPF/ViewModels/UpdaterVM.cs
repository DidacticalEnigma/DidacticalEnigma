using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DidacticalEnigma.Updater.WPF.ViewModels
{
    class UpdaterVM : INotifyPropertyChanged, IDisposable
    {
        private UpdaterProcess updater;

        public event PropertyChangedEventHandler PropertyChanged;

        public UpdateStatus CurrentStatus => updater.CurrentStatus;

        public string CurrentStatusString
        {
            get
            {
                switch (updater.CurrentStatus)
                {
                    case UpdateStatus.DownloadingStatus downloadingStatus:
                        if (downloadingStatus.Percentage == null)
                        {
                            return "Downloading...";
                        }
                        else
                        {
                            return $"Downloading: {downloadingStatus.Percentage.Value}";
                        }

                        break;
                    case UpdateStatus.FailureStatus failureStatus:
                        return $"Failure";
                        break;
                    case UpdateStatus.ProcessingStatus processingStatus:
                        return "Processing...";
                        break;
                    case UpdateStatus.ReadyToStartStatus readyToStartStatus:
                        return "Ready to start.";
                        break;
                    case UpdateStatus.SuccessStatus successStatus:
                        return "Success!";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(CurrentStatus));
                }
            }
        }

        public string Name => updater.Name;
        public ICommand UpdateCommand => updater.UpdateCommand;


        private void ForwardNotificationOfPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(updater.CurrentStatus))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentStatusString)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentStatus)));
            }
        }

        public void Dispose()
        {
            updater.PropertyChanged -= ForwardNotificationOfPropertyChanged;
        }

        public UpdaterVM(UpdaterProcess updater)
        {
            this.updater = updater;
            updater.PropertyChanged += ForwardNotificationOfPropertyChanged;
        }
    }
}
