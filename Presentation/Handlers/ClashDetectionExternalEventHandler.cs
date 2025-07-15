using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.Presentation.Commands;

namespace ClashOpenings.Presentation.Handlers;

public class ClashDetectionExternalEventHandler : IExternalEventHandler
{
    private RevitLinkInstance _linkInstance1;
    private RevitLinkInstance _linkInstance2;
    private Action<string> _updateStatusCallback;

    public void Execute(UIApplication app)
    {
        try
        {
            var uiDoc = app.ActiveUIDocument;
            var command = new SlabsOpeningsCommand();
            string msg = null;
            var elems = new ElementSet();

            var result = command.ExecuteWithSelectedLinks(
                uiDoc,
                ref msg,
                elems,
                _linkInstance1,
                _linkInstance2);

            _updateStatusCallback?.Invoke(result == Result.Succeeded
                ? "Clash detection completed successfully!"
                : "Clash detection failed: " + msg);
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

    public void SetLinks(RevitLinkInstance link1, RevitLinkInstance link2)
    {
        _linkInstance1 = link1;
        _linkInstance2 = link2;
    }

    public void SetStatusCallback(Action<string> callback)
    {
        _updateStatusCallback = callback;
    }
}