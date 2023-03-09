using System;
using System.Windows.Input;

namespace EulerGraph.Controller.Commands;

public class RelayCommand:ICommand
{
    public Action _action;
    public bool CanExecute(object? parameter) => true;

    public RelayCommand(Action action)
    {
        this._action = action;
    }
    public void Execute(object? parameter) => _action?.Invoke();

    public event EventHandler? CanExecuteChanged;
}