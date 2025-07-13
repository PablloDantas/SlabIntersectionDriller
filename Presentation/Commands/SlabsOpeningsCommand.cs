using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.Infrastructure.RevitAPI;

namespace ClashOpenings.Presentation.Commands;

[Transaction(TransactionMode.Manual)]
public class SlabsOpeningsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        var app = commandData.Application;
        var uiApp = app.Application;
        var uiDoc = app.ActiveUIDocument;
        var activeView = doc.ActiveView;

        var repo = new RevitRepository(doc);
        var factory = new RevitBuildingComponentFactory();

        var pipes = repo.GetByCategory(BuiltInCategory.OST_PipeCurves);
        var floors = repo.GetByCategory(BuiltInCategory.OST_Floors);


        var domainPipes = pipes.Select(pipe => factory.CreateAdapter(pipe).ToDomain(pipe));
        var domainFloors = floors.Select(floor => factory.CreateAdapter(floor).ToDomain(floor));


        return Result.Succeeded;
    }
}