using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CatImageRecognizer
{
    public class RelayCommand : ICommand
    {
        public Action Command { get; set; }
        public RelayCommand(Action command)
        {
            Command = command;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Command();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public Action<T> Command { get; set; }


        public RelayCommand(Action<T> command)
        {
            Command = command;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Command((T)parameter);
        }
    }
}

