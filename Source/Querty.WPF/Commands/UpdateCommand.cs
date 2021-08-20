using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Querty.WPF
{
    public class UpdateCommand : ICommand
    {
        private MainViewModel _mainViewModel;

        public UpdateCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return (_mainViewModel.TemplateText.Length > 4) && (_mainViewModel.TemplateText.Length < 200);
        }

        public void Execute(object parameter)
        {
            _mainViewModel.UpdateControls();
        }
    }
}
