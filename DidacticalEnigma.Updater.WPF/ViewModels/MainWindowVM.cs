using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Utility.Utils;

namespace DidacticalEnigma.Updater.WPF.ViewModels
{
    class MainWindowVM : INotifyPropertyChanged
    {
        public ICollection<UpdaterVM> Updaters { get; }

        private RelayCommand updateAllCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand UpdateAllCommand => updateAllCommand;

        public ICommand QuitCommand { get; }

        public string FailureLog
        {
            get
            {
                var sb = new StringBuilder();
                foreach(var updater in Updaters)
                {
                    if(updater.CurrentStatus is UpdateStatus.FailureStatus failureStatus)
                    {
                        sb.Append(updater.Name);
                        sb.Append(": ");
                        sb.Append(failureStatus.Reason);
                        sb.AppendLine();
                        sb.Append(failureStatus.LongMessage);
                        sb.AppendLine();
                    }
                }
                return sb.ToString();
            }
        }

        public MainWindowVM(
            IEnumerable<UpdaterProcess> updaters)
        {
            Updaters = new ObservableBatchCollection<UpdaterVM>(
                updaters.Select(updater => new UpdaterVM(updater)));

            updateAllCommand = new RelayCommand(() =>
            {
                foreach(var updater in Updaters)
                {
                    if(updater.UpdateCommand.CanExecute(null))
                    {
                        updater.UpdateCommand.Execute(null);
                    }
                }
            }, () =>
            {
                return Updaters.All(updater => updater.UpdateCommand.CanExecute(null));
            });

            foreach (var updater in Updaters)
            {
                updater.UpdateCommand.CanExecuteChanged += UpdateAllCommandCanExecuteChanged;
                updater.PropertyChanged += UpdaterPropertyChanged;
            }

            QuitCommand = new RelayCommand(() =>
            {
                App.Current.MainWindow.Close();
            });
        }

        private void UpdaterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(UpdaterVM.CurrentStatus))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FailureLog)));
            }
        }

        private void UpdateAllCommandCanExecuteChanged(object sender, EventArgs e)
        {
            updateAllCommand.OnExecuteChanged();
        }
    }
}
