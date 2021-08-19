using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Querty.WPF
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }        

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute.Invoke((T)parameter);
        }     
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute) : base (_ => execute())
        {

        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null) : base(execute, canExecute)
        {
        }
    }
}
