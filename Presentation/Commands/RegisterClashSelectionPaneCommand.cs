using System.Windows.Input;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.Presentation.ViewModels;
using ClashOpenings.Presentation.Views;

namespace ClashOpenings.Presentation.Commands;

/// <summary>
///     Guid para identificação única do painel ancorado
/// </summary>
public static class ClashSelectionDockablePaneId
{
    public static readonly DockablePaneId Id = new(
        new Guid("{8ED96E66-56A9-4068-871C-69C1B4186E49}"));
}

/// <summary>
///     Variáveis estáticas compartilhadas entre os comandos
/// </summary>
public static class DockablePaneUtility
{
    // Armazena a instância do painel para que possa ser acessada por outros comandos
    public static ClashSelectionDockablePane CurrentPane { get; set; }

    // Armazena a instância do UIDocument atual
    public static UIDocument CurrentUIDocument { get; set; }
}

/// <summary>
///     Comando para inicializar o painel ancorado de seleção de clash (não registra novamente)
/// </summary>
[Transaction(TransactionMode.Manual)]
public class RegisterClashSelectionPaneCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiApp = commandData.Application;
        var uiDoc = uiApp.ActiveUIDocument;

        // Guarda o UIDocument para uso posterior
        DockablePaneUtility.CurrentUIDocument = uiDoc;

        try
        {
            // Se o painel já está registrado, apenas atualizamos o ViewModel
            if (DockablePaneUtility.CurrentPane != null)
            {
                var viewModel = new ClashSelectionViewModel(uiDoc);
                DockablePaneUtility.CurrentPane.SetViewModel(viewModel);

                // Mostra o painel existente
                try
                {
                    var dockablePane = uiApp.GetDockablePane(ClashSelectionDockablePaneId.Id);
                    dockablePane.Show();
                }
                catch
                {
                    // Ignorar se o painel não puder ser mostrado
                }

                return Result.Succeeded;
            }

            // Este comando não registra mais o painel, isso é feito no startup do aplicativo
            // Se o painel não foi registrado no startup por algum motivo, mostra uma mensagem
            message = "O painel não foi registrado corretamente. Reinicie o Revit e tente novamente.";
            return Result.Failed;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}

/// <summary>
///     Comando para mostrar o painel ancorado e atualizar seu ViewModel
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
public class ShowClashSelectionPaneCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;

            // Guarda o UIDocument para uso posterior
            DockablePaneUtility.CurrentUIDocument = uiDoc;

            try
            {
                // Verifica se o painel está registrado
                var dockablePane = uiApp.GetDockablePane(ClashSelectionDockablePaneId.Id);

                // Atualiza o ViewModel se o painel já foi criado
                if (DockablePaneUtility.CurrentPane != null)
                {
                    var viewModel = new ClashSelectionViewModel(uiDoc);
                    DockablePaneUtility.CurrentPane.SetViewModel(viewModel);
                }

                // Mostra o painel
                dockablePane.Show();
            }
            catch (Exception)
            {
                // O painel não está registrado, não podemos mostrá-lo
                message = "O painel não está registrado. Reinicie o Revit e tente novamente.";
                return Result.Failed;
            }

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}

/// <summary>
///     Comando para ocultar o painel ancorado
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
public class HideClashSelectionPaneCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var uiApp = commandData.Application;

            try
            {
                var dockablePane = uiApp.GetDockablePane(ClashSelectionDockablePaneId.Id);
                dockablePane.Hide();
            }
            catch (Exception)
            {
                // O painel não está registrado, ignoramos o erro
            }

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}

/// <summary>
///     Classe RelayCommand para o binding do botão
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Predicate<object> _canExecute;
    private readonly Action<object> _execute;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}