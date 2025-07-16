using Autodesk.Revit.UI;
using ClashOpenings.Presentation.RevitSetup.Ribbon;
using ClashOpenings.Presentation.Vendors.Ricaun;
using ClashOpenings.Presentation.Views;

namespace ClashOpenings.Presentation.RevitSetup;

public class ClashOpeningsApp : IExternalApplication
{
    public static DockablePaneCreatorService? DockablePaneCreatorService { get; private set; }
    public static ClashSelectionDockablePane? SlabsOpeningsPane { get; private set; }
    public static Guid SlabsOpeningsGuid { get; private set; }

    public Result OnStartup(UIControlledApplication application)
    {
        DockablePaneCreatorService = new DockablePaneCreatorService(application);
        DockablePaneCreatorService.Initialize();

        SlabsOpeningsGuid = Guid.NewGuid();
        SlabsOpeningsPane = new ClashSelectionDockablePane();

        application.ControlledApplication.ApplicationInitialized += (_, _) =>
        {
            DockablePaneCreatorService.Register(
                SlabsOpeningsGuid,
                "Slabs Openings",
                SlabsOpeningsPane);
        };

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