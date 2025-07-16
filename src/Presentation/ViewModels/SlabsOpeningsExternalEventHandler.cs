using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.src.Commands;
using ClashOpenings.src.Services.ClashDetection;
using ClashOpenings.src.Services.FamilyInstance;
using ClashOpenings.src.Services.Revit;
using System.Text;

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
                _updateStatusCallback?.Invoke("Error: Please select two different models.");
                return;
            }

            var viewService = new ViewGeometryService(doc);
            var searchVolume = viewService.GetSearchVolume();
            if (searchVolume == null)
            {
                _updateStatusCallback?.Invoke("Error: Unsupported view type. Use a plan view or a 3D view with an active section box.");
                return;
            }

            var elements1 = RevitElementCollector.GetElementsFromLink(doc, _linkInstance1, searchVolume);
            var elements2 = RevitElementCollector.GetElementsFromLink(doc, _linkInstance2, searchVolume);

            var clashDetector = new ClashDetectionService();
            var clashResults = clashDetector.FindClashes((_linkInstance1, elements1), (_linkInstance2, elements2));

            var familyPlacer = new FamilyPlacementService(doc);
            var openingsCreated = familyPlacer.CreateOpenings(clashResults);

            var summary = new StringBuilder();
            summary.AppendLine($"Clash detection completed. Found {clashResults.Count} clashes.");
            if (openingsCreated > 0)
            {
                summary.AppendLine($"{openingsCreated} openings were created successfully.");
            }
            _updateStatusCallback?.Invoke(summary.ToString());
        }
        catch (Exception ex)
        {
            _updateStatusCallback?.Invoke($"Error: {ex.Message}");
            TaskDialog.Show("Error", $"Failed to run clash detection: {ex.Message}");
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
