using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.src.Presentation.RevitSetup;
using ClashOpenings.src.Presentation.Vendors.Ricaun;
using ClashOpenings.src.Presentation.ViewModels;

namespace ClashOpenings.src.Commands;

/// <summary>
///     Comando do Revit para detectar colisões entre elementos de diferentes modelos vinculados.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class SlabsOpeningsCommand : IExternalCommand
{
    /// <summary>
    ///     Ponto de entrada principal para o comando.
    /// </summary>
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