using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace ClashOpenings.Presentation.Commands;

/// <summary>
///     Comando do Revit para detectar colisões entre elementos de diferentes modelos vinculados.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class SlabsOpeningsCommand : IExternalCommand
{
    /// <summary>
    ///     Ponto de entrada principal para o comando.
    /// </summary>
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiApp = commandData.Application;

        try
        {
            // Verifica se o painel já está registrado 
            var dockablePane = uiApp.GetDockablePane(ClashSelectionDockablePaneId.Id);
            // O painel já está registrado, então apenas mostramos
            dockablePane.Show();
        }
        catch (Exception)
        {
            // O painel ainda não foi registrado
            try
            {
                // Registramos o painel apenas uma vez
                if (DockablePaneUtility.CurrentPane == null)
                {
                    var registerCommand = new RegisterClashSelectionPaneCommand();
                    var result = registerCommand.Execute(commandData, ref message, elements);

                    if (result != Result.Succeeded) return result;
                }

                // Mostramos o painel
                var dockPane = uiApp.GetDockablePane(ClashSelectionDockablePaneId.Id);
                dockPane.Show();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        return Result.Succeeded;
    }

    /// <summary>
    ///     Método que executa a detecção de colisão com os links selecionados
    /// </summary>
    public Result ExecuteWithSelectedLinks(
        UIDocument uiDoc,
        ref string message,
        ElementSet elements,
        RevitLinkInstance selectedLinkInstance1,
        RevitLinkInstance selectedLinkInstance2)
    {
        // O resto do código permanece igual
        var doc = uiDoc.Document;
        var activeView = doc.ActiveView;

        if (selectedLinkInstance1 == null || selectedLinkInstance2 == null)
        {
            TaskDialog.Show("Erro", "Dois modelos de link devem ser selecionados.");
            return Result.Failed;
        }

        // 2. Obtém a elevação do ponto base do projeto para cálculos de coordenadas.
        var basePoints = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
            .WhereElementIsNotElementType()
            .ToElements();
        double bpElevation = 0;
        foreach (var bp in basePoints)
        {
            var param = bp.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
            if (param != null && param.HasValue)
            {
                bpElevation = param.AsDouble();
                break;
            }
        }

// 3. Cria um filtro para incluir apenas categorias visíveis na vista ativa.
        var categoryIds = doc.Settings.Categories.Cast<Category>().Select(c => c.Id);
        var visibleCatFilters = categoryIds
            .Where(id => !activeView.GetCategoryHidden(id))
            .Select(id => new ElementCategoryFilter(id))
            .Cast<ElementFilter>()
            .ToList();
        var catFilter = new LogicalOrFilter(visibleCatFilters);

        // 4. Define o volume de busca (Outline) com base no tipo de vista ativa.
        Outline worldOutline;
        if (activeView is ViewPlan viewPlan)
        {
            var cropBox = viewPlan.CropBox;
            var vr = viewPlan.GetViewRange();
            var topZ = GetPlaneZ(doc, vr, PlanViewPlane.TopClipPlane, bpElevation);
            var bottomZ = GetPlaneZ(doc, vr, PlanViewPlane.BottomClipPlane, bpElevation);
            var newMin = new XYZ(cropBox.Min.X, cropBox.Min.Y, bottomZ);
            var newMax = new XYZ(cropBox.Max.X, cropBox.Max.Y, topZ);
            worldOutline = new Outline(newMin, newMax);
        }
        else if (activeView is View3D view3D && view3D.IsSectionBoxActive)
        {
            var sectBox = view3D.GetSectionBox();
            var tr = sectBox.Transform;
            var corners = new List<XYZ>();
            var min = sectBox.Min;
            var max = sectBox.Max;
            var xs = new[] { min.X, max.X };
            var ys = new[] { min.Y, max.Y };
            var zs = new[] { min.Z, max.Z };
            foreach (var x in xs)
            foreach (var y in ys)
            foreach (var z in zs)
                corners.Add(tr.OfPoint(new XYZ(x, y, z)));
            double xMin = corners.Min(p => p.X), xMax = corners.Max(p => p.X);
            double yMin = corners.Min(p => p.Y), yMax = corners.Max(p => p.Y);
            double zMin = corners.Min(p => p.Z), zMax = corners.Max(p => p.Z);
            worldOutline = new Outline(new XYZ(xMin, yMin, zMin), new XYZ(xMax, yMax, zMax));
        }
        else
        {
            TaskDialog.Show("Erro", "Tipo de vista não suportado. Use vista de planta ou 3D com section box ativa.");
            return Result.Failed;
        }

        // 5. Coleta elementos de cada link que estão dentro do volume de busca.
        var allCollected = new Dictionary<string, (RevitLinkInstance, List<Element>)>();
        foreach (var linkInst in new[] { selectedLinkInstance1, selectedLinkInstance2 })
        {
            var linkDoc = linkInst.GetLinkDocument();
            if (linkDoc == null) continue;

            var transform = linkInst.GetTotalTransform();
            var inv = transform.Inverse;
            var linkMin = inv.OfPoint(worldOutline.MinimumPoint);
            var linkMax = inv.OfPoint(worldOutline.MaximumPoint);
            var linkOutline = new Outline(linkMin, linkMax);
            var bbFilter = new BoundingBoxIntersectsFilter(linkOutline);
            var filter = new LogicalAndFilter(catFilter, bbFilter);
            var collected = new FilteredElementCollector(linkDoc)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToList();

            if (collected.Count > 0) allCollected.Add(linkDoc.Title, (linkInst, collected));
        }

        // 6. Realiza a detecção de colisão e armazena os resultados em um dicionário.
        var clashResults = new Dictionary<XYZ, (Element, Element)>();
        var linkKeys = allCollected.Keys.ToList();

        for (var i = 0; i < linkKeys.Count; i++)
        for (var j = i + 1; j < linkKeys.Count; j++)
        {
            var (linkInst1, elements1) = allCollected[linkKeys[i]];
            var (linkInst2, elements2) = allCollected[linkKeys[j]];

            var transform1 = linkInst1.GetTotalTransform();
            var transform2 = linkInst2.GetTotalTransform();
            var transform2to1 = transform1.Inverse.Multiply(transform2);

            foreach (var elem1 in elements1)
            {
                var solid1 = GetSolidFromElement(elem1);
                if (solid1 == null || solid1.Volume < 1e-9) continue;

                foreach (var elem2 in elements2)
                {
                    var solid2 = GetSolidFromElement(elem2);
                    if (solid2 == null || solid2.Volume < 1e-9) continue;

                    var transformedSolid2 = SolidUtils.CreateTransformed(solid2, transform2to1);

                    try
                    {
                        var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                            solid1, transformedSolid2, BooleanOperationsType.Intersect);

                        if (intersection != null && intersection.Volume > 1e-9)
                        {
                            var topFace = GetTopFace(intersection);
                            if (topFace == null) continue;

                            var bboxUV = topFace.GetBoundingBox();
                            var centerUV = (bboxUV.Min + bboxUV.Max) / 2.0;
                            var localCenterPoint = topFace.Evaluate(centerUV);

                            var worldCenterPoint = transform1.OfPoint(localCenterPoint);

                            clashResults[worldCenterPoint] = (elem1, elem2);
                        }
                    }
                    catch
                    {
                        /* Ignorar falhas na operação booleana */
                    }
                }
            }
        }

        // 7. Extrai a espessura das lajes/pisos e o diâmetro dos tubos/conduítes envolvidos nas colisões.
        var clashInformation = new Dictionary<XYZ, (double thickness, double diameter)>();
        foreach (var clash in clashResults)
        {
            var elem1 = clash.Value.Item1;
            var elem2 = clash.Value.Item2;

            double thickness = 0;
            double diameter = 0;

            var floor = elem1 as Floor ?? elem2 as Floor;
            var mepCurve = elem1 as MEPCurve ?? elem2 as MEPCurve;

            if (floor != null)
            {
                var thicknessParam = floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);
                if (thicknessParam != null && thicknessParam.HasValue)
                    thickness = thicknessParam.AsDouble();
            }

            if (mepCurve != null)
            {
                var dParam = mepCurve.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM) ??
                             mepCurve.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                if (dParam != null && dParam.HasValue)
                    diameter = dParam.AsDouble();
            }

            if (thickness > 0 || diameter > 0)
                clashInformation.Add(clash.Key, (thickness, diameter));
        }

        // 8. Exibe um resumo dos resultados da detecção de colisão.
        var summary = new StringBuilder();
        summary.AppendLine("Detecção de Conflitos Concluída");
        summary.AppendLine($"Total de conflitos encontrados: {clashResults.Count}");

        if (clashResults.Count > 0)
        {
            using var t = new Transaction(doc, "Criar Furos de Laje");
            t.Start();

            var familySymbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(fs =>
                    fs.Family.Name == "FURO-QUADRADO-LAJE" && fs.Name == "SDR - Furo na laje");

            if (familySymbol != null)
            {
                if (!familySymbol.IsActive) familySymbol.Activate();

                foreach (var clash in clashInformation)
                {
                    // Adiciona um deslocamento de 5cm em Z no ponto de inserção
                    var zOffset = UnitUtils.ConvertToInternalUnits(5, UnitTypeId.Centimeters);
                    var insertionPoint = clash.Key.Add(new XYZ(0, 0, zOffset));

                    var instance = doc.Create.NewFamilyInstance(insertionPoint, familySymbol,
                        StructuralType.NonStructural);
                    var (thickness, diameter) = clash.Value;

                    if (thickness > 0)
                    {
                        // Adiciona uma folga de 10cm usando a conversão de unidades do Revit
                        var clearance = UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Centimeters);
                        var finalThickness = thickness + clearance;

                        var thicknessParam = instance.LookupParameter("FUR.esp-laje");
                        thicknessParam?.Set(finalThickness);
                    }

                    if (diameter > 0)
                    {
                        // Adiciona uma folga de 1cm usando a conversão de unidades do Revit
                        var clearance = UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters);
                        var finalDiameter = diameter + clearance;

                        var dim1Param = instance.LookupParameter("TH-FUR-DIM1");
                        dim1Param?.Set(finalDiameter);
                        var dim2Param = instance.LookupParameter("TH-FUR-DIM2");
                        dim2Param?.Set(finalDiameter);
                    }
                }

                summary.AppendLine($"\n{clashInformation.Count} furos foram criados com sucesso.");
            }
            else
            {
                summary.AppendLine(
                    "\nA família de furos 'FURO-QUADRADO-LAJE' com o tipo 'SDR - Furo na laje' não foi encontrada.");
            }

            t.Commit();
        }

        TaskDialog.Show("Resultado da Detecção de Conflitos", summary.ToString());

        return Result.Succeeded;
    }

    // Métodos auxiliares permanecem iguais
    private PlanarFace GetTopFace(Solid solid)
    {
        // Implementação original...
        PlanarFace topFace = null;
        var highestZ = double.MinValue;

        foreach (Face face in solid.Faces)
            if (face is PlanarFace planarFace && planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ))
                if (planarFace.Origin.Z > highestZ)
                {
                    highestZ = planarFace.Origin.Z;
                    topFace = planarFace;
                }

        return topFace;
    }

    private Solid GetSolidFromElement(Element element)
    {
        // Implementação original...
        var options = new Options { ComputeReferences = true, DetailLevel = ViewDetailLevel.Fine };
        var geomElem = element.get_Geometry(options);
        if (geomElem == null) return null;

        foreach (var geomObj in geomElem)
        {
            if (geomObj is Solid solid && solid.Volume > 0) return solid;
            if (geomObj is GeometryInstance geomInst)
                foreach (var nestedGeomObj in geomInst.GetInstanceGeometry())
                    if (nestedGeomObj is Solid nestedSolid && nestedSolid.Volume > 0)
                        return nestedSolid;
        }

        return null;
    }

    private double GetPlaneZ(Document doc, PlanViewRange vr, PlanViewPlane plane, double bpElevation)
    {
        // Implementação original...
        try
        {
            var level = doc.GetElement(vr.GetLevelId(plane)) as Level;
            return level.Elevation + vr.GetOffset(plane) - bpElevation;
        }
        catch
        {
            var cutLevel = doc.GetElement(vr.GetLevelId(PlanViewPlane.CutPlane)) as Level;
            return cutLevel.Elevation + vr.GetOffset(PlanViewPlane.CutPlane) - bpElevation;
        }
    }
}