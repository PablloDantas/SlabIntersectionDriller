using Autodesk.Revit.UI;
using ClashOpenings.Presentation.RevitSetup.Ribbon;

namespace ClashOpenings.Presentation.RevitSetup;

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