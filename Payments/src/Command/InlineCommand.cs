using System;
using System.Windows.Input;

namespace Payments.Command {
    public class InlineCommand : ICommand {
        private readonly Action<object> _action;

        public InlineCommand(Action<object> action) {
            _action = action;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            _action(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}