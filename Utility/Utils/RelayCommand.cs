using System;
using System.Windows.Input;

namespace Utility.Utils
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> action;

        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action action, Func<bool> canExecute = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            this.action = (parameter) => action();
            canExecute = canExecute ?? (() => true);
            this.canExecute = (parameter) => canExecute();
        }

        public RelayCommand(Action<object> action, Func<object, bool> canExecute = null)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.canExecute = canExecute ?? (p => true);
        }

        public event EventHandler CanExecuteChanged;

        public void OnExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public bool CanExecute(object parameter)
        {
            return canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}