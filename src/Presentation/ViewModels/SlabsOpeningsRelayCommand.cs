using System.Windows.Input;

namespace ClashOpenings.src.Presentation.ViewModels;

public class SlabsOpeningsRelayCommand : ICommand
{
    private readonly Predicate<object>? _canExecute;
    private readonly Action<object> _execute;

    public SlabsOpeningsRelayCommand(Action<object> execute, Predicate<object>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter!);
    }

    public void Execute(object? parameter)
    {
        _execute(parameter!);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}