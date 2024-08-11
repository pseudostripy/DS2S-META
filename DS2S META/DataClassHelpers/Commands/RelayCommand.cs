using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DS2S_META.DataClassHelpers.Commands
{
    public class RelayCommand : ICommand
    {
        readonly Action<object?> _executeMethod;
        readonly Func<object?, bool> _canexecuteMethod;

        public RelayCommand(Action<object?> executeMethod, Func<object?, bool> canexecuteMethod)
        {
            _executeMethod = executeMethod;
            _canexecuteMethod = canexecuteMethod;
        }

        public bool CanExecute(object? parameter)
        {
            if (_canexecuteMethod != null)
            {
                return _canexecuteMethod(parameter);
            }
            else
            {
                return false;
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object? parameter)
        {
            _executeMethod(parameter);
        }
    }
}
