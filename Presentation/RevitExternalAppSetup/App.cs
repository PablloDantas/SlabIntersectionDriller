using Autodesk.Revit.UI;
using ClashOpenings.Presentation.RevitExternalAppSetup.Ribbon;

namespace ClashOpenings.Presentation.RevitExternalAppSetup;

public class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        var ribbonBuilder = new RibbonBuilder(application);
        ribbonBuilder.BuildRibbon();

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }
}