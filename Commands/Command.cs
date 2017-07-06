using System;
using System.Windows.Input;

namespace clipman.Commands
{
    class Command : ICommand
    {
        private readonly Action<object> action;

        public Command(Action<object> action)
        {
            this.action = action;
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
