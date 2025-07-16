using System.Windows.Input;

namespace ClashOpenings.src.Presentation.ViewModels;

/// <summary>
///     Implementação de <see cref="ICommand" /> que permite encapsular métodos
///     como comandos executáveis e controlar sua disponibilidade.
/// </summary>
public class SlabsOpeningsRelayCommand : ICommand
{
    /// <summary>
    ///     O predicado opcional para determinar se o comando pode ser executado.
    /// </summary>
    private readonly Predicate<object>? _canExecute;

    /// <summary>
    ///     A ação a ser executada quando o comando é invocado.
    /// </summary>
    private readonly Action<object> _execute;

    /// <summary>
    ///     Inicializa uma nova instância da classe <see cref="SlabsOpeningsRelayCommand" />.
    /// </summary>
    /// <param name="execute">A ação a ser executada pelo comando.</param>
    /// <param name="canExecute">
    ///     Um predicado opcional que define o método para determinar se o comando pode executar no seu estado atual.
    /// </param>
    /// <exception cref="ArgumentNullException">Lançada se <paramref name="execute" /> for nulo.</exception>
    public SlabsOpeningsRelayCommand(Action<object> execute, Predicate<object>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    ///     Ocorre quando as condições que afetam se o comando deve ser executado são alteradas.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    ///     Define o método que determina se o comando pode executar no seu estado atual.
    /// </summary>
    /// <param name="parameter">
    ///     Dados usados pelo comando. Se o comando não exige que dados sejam passados, este objeto pode ser definido como
    ///     nulo.
    /// </param>
    /// <returns><see langword="true" /> se este comando pode ser executado; caso contrário, <see langword="false" />.</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter!);
    }

    /// <summary>
    ///     Define o método a ser invocado quando o comando é chamado.
    /// </summary>
    /// <param name="parameter">
    ///     Dados usados pelo comando. Se o comando não exige que dados sejam passados, este objeto pode ser definido como
    ///     nulo.
    /// </param>
    public void Execute(object? parameter)
    {
        _execute(parameter!);
    }
}