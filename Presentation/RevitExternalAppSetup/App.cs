using Autodesk.Revit.UI;
using ClashOpenings.Presentation.RevitExternalAppSetup.Ribbon;
using ClashOpenings.Presentation.Vendors.Ricaun;
using ClashOpenings.Presentation.Views;

namespace ClashOpenings.Presentation.RevitExternalAppSetup;

public class App : IExternalApplication
{
    public static DockablePaneCreatorService? DockablePaneCreatorService { get; private set; }
    public static ClashSelectionDockablePane? SlabsOpeningsPane { get; private set; }
    public static Guid SlabsOpeningsGuid { get; private set; }

    public Result OnStartup(UIControlledApplication application)
    {
        DockablePaneCreatorService = new DockablePaneCreatorService(application);
        DockablePaneCreatorService.Initialize();


        application.ControlledApplication.ApplicationInitialized += (_, _) =>
        {
            var slabOpeningsGuid = Guid.NewGuid();
            SlabsOpeningsGuid = slabOpeningsGuid;

            var slabOpeningsPane = new ClashSelectionDockablePane();
            SlabsOpeningsPane = slabOpeningsPane;

            DockablePaneCreatorService.Register(
                slabOpeningsGuid,
                "Slabs Openings",
                SlabsOpeningsPane);
        };

        // Cria a interface da ribbon
        var ribbonBuilder = new RibbonBuilder(application);
        ribbonBuilder.BuildRibbon();

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        DockablePaneCreatorService?.Dispose();

        return Result.Succeeded;
    }
}