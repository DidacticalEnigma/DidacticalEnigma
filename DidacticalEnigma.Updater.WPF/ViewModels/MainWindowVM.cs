using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Utility.Utils;

namespace DidacticalEnigma.Updater.WPF.ViewModels
{
    class MainWindowVM
    {
        public ICollection<UpdaterVM> Updaters { get; }

        private RelayCommand updateAllCommand;

        public ICommand UpdateAllCommand => updateAllCommand;

        public ICommand QuitCommand { get; }

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
            }

            QuitCommand = new RelayCommand(() =>
            {
                App.Current.MainWindow.Close();
            });
        }

        private void UpdateAllCommandCanExecuteChanged(object sender, EventArgs e)
        {
            updateAllCommand.OnExecuteChanged();
        }
    }
}
