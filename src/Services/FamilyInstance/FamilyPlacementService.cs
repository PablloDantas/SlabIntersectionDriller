using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace ClashOpenings.src.Services.FamilyInstance
{
    public class FamilyPlacementService
    {
        private readonly Document _doc;

        public FamilyPlacementService(Document doc)
        {
            _doc = doc;
        }

        public int CreateOpenings(Dictionary<XYZ, (double thickness, double diameter)> clashInformation)
        {
            var familySymbol = new FilteredElementCollector(_doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(fs =>
                    fs.Family.Name == "FURO-QUADRADO-LAJE" && fs.Name == "SDR - Furo na laje");

            if (familySymbol == null)
            {
                TaskDialog.Show("Error", "Opening family 'FURO-QUADRADO-LAJE' with type 'SDR - Furo na laje' not found.");
                return 0;
            }

            if (!familySymbol.IsActive)
            {
                familySymbol.Activate();
            }

            var createdCount = 0;
            using (var t = new Transaction(_doc, "Create Slab Openings"))
            {
                t.Start();

                foreach (var clash in clashInformation)
                {
                    var zOffset = UnitUtils.ConvertToInternalUnits(5, UnitTypeId.Centimeters);
                    var insertionPoint = clash.Key.Add(new XYZ(0, 0, zOffset));

                    var instance = _doc.Create.NewFamilyInstance(insertionPoint, familySymbol, StructuralType.NonStructural);
                    var (thickness, diameter) = clash.Value;

                    if (thickness > 0)
                    {
                        var clearance = UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Centimeters);
                        var finalThickness = thickness + clearance;
                        var thicknessParam = instance.LookupParameter("FUR.esp-laje");
                        thicknessParam?.Set(finalThickness);
                    }

                    if (diameter > 0)
                    {
                        var clearance = UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters);
                        var finalDiameter = diameter + clearance;
                        var dim1Param = instance.LookupParameter("TH-FUR-DIM1");
                        dim1Param?.Set(finalDiameter);
                        var dim2Param = instance.LookupParameter("TH-FUR-DIM2");
                        dim2Param?.Set(finalDiameter);
                    }
                    createdCount++;
                }

                t.Commit();
            }
            return createdCount;
        }
    }
}
