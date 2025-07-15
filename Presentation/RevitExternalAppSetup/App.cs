using Autodesk.Revit.UI;
using ClashOpenings.Presentation.Commands;
using ClashOpenings.Presentation.RevitExternalAppSetup.Ribbon;
using ClashOpenings.Presentation.Views;

namespace ClashOpenings.Presentation.RevitExternalAppSetup;

public class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        // Registra o painel ancorado durante o startup do Revit
        RegisterDockablePane(application);

        // Cria a interface da ribbon
        var ribbonBuilder = new RibbonBuilder(application);
        ribbonBuilder.BuildRibbon();

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }

    private void RegisterDockablePane(UIControlledApplication application)
    {
        try
        {
            // Cria e registra o painel ancorado apenas uma vez durante o startup
            var pane = new ClashSelectionDockablePane();
            DockablePaneUtility.CurrentPane = pane;

            application.RegisterDockablePane(
                ClashSelectionDockablePaneId.Id,
                "Clash Selection",
                pane);
        }
        catch (Exception)
        {
            // Ignora erros, como o painel já estar registrado
        }
    }
}