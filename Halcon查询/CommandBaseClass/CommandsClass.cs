using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyCommands
{
    public class CommandsBaseClass : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (CanExecuteFunc == null)
                return true;
            return CanExecuteFunc.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            if (ExecuteAction == null)
                return;
            ExecuteAction.Invoke(parameter);
        }

        public void RasieCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged.Invoke(this, EventArgs.Empty);
        }
        public void RasieCanExecuteChanged(object param)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged.Invoke(this, EventArgs.Empty);
        }
        public Action<object> ExecuteAction;
        public Func<object, bool> CanExecuteFunc;
    }
}
