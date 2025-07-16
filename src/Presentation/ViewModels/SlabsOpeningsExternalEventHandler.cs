using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.src.Models;
using ClashOpenings.src.Services;

namespace ClashOpenings.src.Presentation.ViewModels;

public class SlabsOpeningsExternalEventHandler : IExternalEventHandler
{
    private RevitLinkInstance? _linkInstance1;
    private RevitLinkInstance? _linkInstance2;
    private Action<string>? _updateStatusCallback;

    public void Execute(UIApplication app)
    {
        try
        {
            var uiDoc = app.ActiveUIDocument;
            var doc = uiDoc.Document;

            if (_linkInstance1 == null || _linkInstance2 == null)
            {
                _updateStatusCallback?.Invoke("Error: Por favor, selecione os DOIS modelos e tente novamente.");
                return;
            }

            if (_linkInstance1.Id == _linkInstance2.Id)
            {
                _updateStatusCallback?.Invoke("Error: O Mesmo modelo foi selecionado duas vezes. " +
                                              "Por favor, tente novamente com modelos diferentes.");
                return;
            }


            var viewService = new ViewGeometryService(doc);
            var searchVolume = viewService.GetSearchVolume();
            if (searchVolume == null)
            {
                _updateStatusCallback?.Invoke(
                    "Error: Tipo de vista não suportado. Use uma vista de planta baixa ou uma vista 3D," +
                    " com a região de recorte e a caixa de corte ativa.");
                return;
            }

            var elements1 = ElementCollector.GetElementsFromLink(doc, _linkInstance1, searchVolume);
            var linkElementData1 = new LinkElementData(_linkInstance1, elements1);


            var elements2 = ElementCollector.GetElementsFromLink(doc, _linkInstance2, searchVolume);
            var linkElementData2 = new LinkElementData(_linkInstance2, elements2);


            var clashDetector = new ClashDetective();
            var clashResults = clashDetector.FindClashes(linkElementData1, linkElementData2);

            var familyPlacer = new FamilyCreator(doc);
            var openingsCreated = familyPlacer.CreateOpenings(clashResults);

            var summary = new StringBuilder();
            summary.AppendLine($"Detecção de conflitos completa. Encontramos {clashResults.Count} conflitos.");
            if (openingsCreated > 0) summary.AppendLine($"{openingsCreated} furações foram criadas com sucesso.");
            _updateStatusCallback?.Invoke(summary.ToString());
        }
        catch (Exception ex)
        {
            _updateStatusCallback?.Invoke($"Error: {ex.Message}");
            TaskDialog.Show("Error", $"Falha na execução da detecção de conflitos: {ex.Message}");
        }
    }

    public string GetName()
    {
        return "Clash Detection External Event Handler";
    }

    public void SetLinks(RevitLinkInstance? link1, RevitLinkInstance? link2)
    {
        _linkInstance1 = link1;
        _linkInstance2 = link2;
    }

    public void SetStatusCallback(Action<string>? callback)
    {
        _updateStatusCallback = callback;
    }
}