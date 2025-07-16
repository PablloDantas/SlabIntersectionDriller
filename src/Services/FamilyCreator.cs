using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using ClashOpenings.src.Models;

namespace ClashOpenings.src.Services;

public class FamilyCreator(Document doc)
{
    public int CreateOpenings(List<ClashResult> clashResults)
    {
        var familySymbol = FindAndActivateOpeningFamilySymbol();

        if (familySymbol == null)
        {
            TaskDialog.Show("Erro",
                "Família de furação 'FURO-QUADRADO-LAJE' com tipo 'SDR - Furo na laje' não encontrada.");
            return 0;
        }

        var createdCount = 0;
        using (var t = new Transaction(doc, "Criar Furações em Lajes"))
        {
            t.Start();

            createdCount += clashResults.Count(clash => TryCreateSingleOpening(clash, familySymbol));

            t.Commit();
        }

        return createdCount;
    }

    private FamilySymbol? FindAndActivateOpeningFamilySymbol()
    {
        var familySymbol = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .FirstOrDefault(fs =>
                fs.Family.Name == "FURO-QUADRADO-LAJE" && fs.Name == "SDR - Furo na laje");

        if (familySymbol != null && !familySymbol.IsActive)
        {
            familySymbol.Activate();
            doc.Regenerate(); // A regeneração pode ser necessária após ativar um símbolo
        }

        return familySymbol;
    }

    private bool TryCreateSingleOpening(ClashResult clash, FamilySymbol familySymbol)
    {
        var zOffset = UnitUtils.ConvertToInternalUnits(5, UnitTypeId.Centimeters);
        var insertionPoint = clash.CenterPoint.Add(new XYZ(0, 0, zOffset));

        var instance = doc.Create.NewFamilyInstance(insertionPoint, familySymbol, StructuralType.NonStructural);

        if (instance == null) return false;

        SetOpeningParameters(instance, clash);
        return true;
    }

    private static void SetOpeningParameters(FamilyInstance instance, ClashResult clash)
    {
        if (clash.Thickness > 0) SetThicknessParameters(instance, clash);

        if (clash.Diameter > 0) SetDiameterParameters(instance, clash);
    }

    private static void SetThicknessParameters(FamilyInstance instance, ClashResult clash)
    {
        var clearance = UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Centimeters);
        var finalThickness = clash.Thickness + clearance;

        var thicknessParam = instance.LookupParameter("FUR.esp-laje");
        thicknessParam?.Set(finalThickness);

        var floorIdParam = instance.LookupParameter("FUR.ESTRUTURA.ID");

        floorIdParam?.Set(clash.GetFloor()?.Id.Value ?? ElementId.InvalidElementId.Value);
    }

    private static void SetDiameterParameters(FamilyInstance instance, ClashResult clash)
    {
        var clearance = UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters);
        var finalDiameter = clash.Diameter + clearance;

        var dim1Param = instance.LookupParameter("TH-FUR-DIM1");
        dim1Param?.Set(finalDiameter);
        var dim2Param = instance.LookupParameter("TH-FUR-DIM2");
        dim2Param?.Set(finalDiameter);

        var curveIdParam = instance.LookupParameter("FUR.TUBO.ID");

        curveIdParam?.Set(clash.GetMepCurve()?.Id.Value ?? ElementId.InvalidElementId.Value);
    }
}