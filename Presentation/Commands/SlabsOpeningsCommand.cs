using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ClashOpenings.Presentation.Commands;

[Transaction(TransactionMode.Manual)]
public class SlabsOpeningsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // Lógica do comando aqui
        TaskDialog.Show("Info", "Comando executado com sucesso!");
        return Result.Succeeded;
    }
}
