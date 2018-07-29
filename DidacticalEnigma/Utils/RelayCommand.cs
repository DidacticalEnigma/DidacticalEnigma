using System;
using System.Windows.Input;

namespace DidacticalEnigma
{
    internal class RelayCommand : ICommand
    {
        private Action p;

        public RelayCommand(Action p)
        {
            this.p = p;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            p();
        }
    }
}