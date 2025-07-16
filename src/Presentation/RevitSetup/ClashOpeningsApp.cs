using Autodesk.Revit.UI;
using ClashOpenings.src.Presentation.RevitSetup.Ribbon;
using ClashOpenings.src.Presentation.Vendors.Ricaun;
using ClashOpenings.src.Presentation.Views;

namespace ClashOpenings.src.Presentation.RevitSetup;

public class ClashOpeningsApp : IExternalApplication
{
    public static DockablePaneCreatorService? DockablePaneCreatorService { get; private set; }
    public static ClashSelectionView? SlabsOpeningsPane { get; private set; }
    public static Guid SlabsOpeningsGuid { get; private set; }

    public Result OnStartup(UIControlledApplication application)
    {
        DockablePaneCreatorService = new DockablePaneCreatorService(application);
        DockablePaneCreatorService.Initialize();

        SlabsOpeningsGuid = Guid.NewGuid();
        SlabsOpeningsPane = new ClashSelectionView();

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