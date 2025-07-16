using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.src.Presentation.RevitSetup;
using ClashOpenings.src.Presentation.Vendors.Ricaun;
using ClashOpenings.src.Presentation.ViewModels;
using ClashOpenings.src.Services.ClashDetection;
using ClashOpenings.src.Services.FamilyInstance;
using ClashOpenings.src.Services.Revit;

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

    /// <summary>
    ///     Método que executa a detecção de colisão com os links selecionados
    /// </summary>
    public Result ExecuteWithSelectedLinks(
        UIDocument uiDoc,
        ref string? message,
        ElementSet elements,
        RevitLinkInstance? selectedLinkInstance1,
        RevitLinkInstance? selectedLinkInstance2)
    {
        var doc = uiDoc.Document;

        if (selectedLinkInstance1 == null || selectedLinkInstance2 == null)
        {
            message = "Dois modelos de link devem ser selecionados.";
            TaskDialog.Show("Erro", message);
            return Result.Failed;
        }

        var viewService = new ViewGeometryService(doc);
        var searchVolume = viewService.GetSearchVolume();
        if (searchVolume == null)
        {
            message = "Tipo de vista não suportado. Use vista de planta ou 3D com section box ativa.";
            return Result.Failed;
        }

        var elements1 = RevitElementCollector.GetElementsFromLink(doc, selectedLinkInstance1, searchVolume);
        var elements2 = RevitElementCollector.GetElementsFromLink(doc, selectedLinkInstance2, searchVolume);

        var clashDetector = new ClashDetectionService();
        var clashResults = clashDetector.FindClashes((selectedLinkInstance1, elements1), (selectedLinkInstance2, elements2));

        var clashInfo = clashDetector.ProcessClashResults(clashResults);

        var familyPlacer = new FamilyPlacementService(doc);
        var openingsCreated = familyPlacer.CreateOpenings(clashInfo);

        var summary = new StringBuilder();
        summary.AppendLine("Detecção de Conflitos Concluída");
        summary.AppendLine($"Total de conflitos encontrados: {clashResults.Count}");
        if (openingsCreated > 0)
        {
            summary.AppendLine($"\n{openingsCreated} furos foram criados com sucesso.");
        }
        else if (clashResults.Count > 0)
        {
            summary.AppendLine("\nNenhum furo foi criado. Verifique se a família de furos está carregada e se os parâmetros estão corretos.");
        }

        TaskDialog.Show("Resultado da Detecção de Conflitos", summary.ToString());

        return Result.Succeeded;
    }
}