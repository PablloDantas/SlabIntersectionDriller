using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.Presentation.RevitSetup;
using ClashOpenings.Presentation.Vendors.Ricaun;
using ClashOpenings.Presentation.ViewModels;

namespace ClashOpenings.Commands;

/// <summary>
///     Representa o comando principal do Revit para gerenciar a interface do usuário de detecção de conflitos de
///     aberturas.
///     Este comando é responsável por inicializar e exibir ou ocultar o painel acoplável da funcionalidade.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class SlabsOpeningsCommand : IExternalCommand
{
    /// <summary>
    ///     Executa o comando principal do Revit.
    ///     Este método inicializa o ViewModel, define-o para o painel acoplável de detecção de conflitos
    ///     e alterna a visibilidade do painel acoplável.
    /// </summary>
    /// <param name="commandData">Contém informações sobre a aplicação Revit, como o UIDocument ativo.</param>
    /// <param name="message">Uma mensagem de retorno do comando em caso de falha.</param>
    /// <param name="elements">Um conjunto de elementos que podem ser selecionados pelo usuário, não utilizado neste comando.</param>
    /// <returns>O resultado da execução do comando, indicando sucesso ou falha.</returns>
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiApp = commandData.Application;
        var uiDoc = uiApp.ActiveUIDocument;

        var viewModel = new SlabsOpeningsViewModel(uiDoc);
        ClashOpeningsApp.SlabsOpeningsPane?.SetViewModel(viewModel);

        var dockablePane = ClashOpeningsApp.DockablePaneCreatorService?.Get(ClashOpeningsApp.SlabsOpeningsGuid);

        if (dockablePane != null && dockablePane.TryIsShow())
            dockablePane.TryHide();
        else
            dockablePane?.TryShow();

        return Result.Succeeded;
    }
}