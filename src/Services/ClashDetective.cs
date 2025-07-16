using Autodesk.Revit.DB;
using ClashOpenings.src.Models;

// Adicionar o using para LinkElementData

// Para Console.WriteLine

namespace ClashOpenings.src.Services;

public class ClashDetective
{
    private const double MinVolumeTolerance = 1e-9;
    private const double MinClashParameterValue = 0;

    public List<ClashResult> FindClashes(LinkElementData linkData1, LinkElementData linkData2)
    {
        var clashResults = new List<ClashResult>();

        var (transform1, transform2, transform2To1) = CalculateClashTransforms(linkData1, linkData2);

        foreach (var elem1 in linkData1.Elements)
        {
            var solid1 = GetSolidFromElement(elem1);
            if (solid1 == null || solid1.Volume < MinVolumeTolerance) continue;

            foreach (var elem2 in linkData2.Elements)
            {
                var solid2 = GetSolidFromElement(elem2);
                if (solid2 == null || solid2.Volume < MinVolumeTolerance) continue;

                ProcessElementPair(elem1, solid1, elem2, solid2, transform2To1, transform1, clashResults);
            }
        }

        return clashResults;
    }

    private (Transform transform1, Transform transform2, Transform transform2To1) CalculateClashTransforms(
        LinkElementData linkData1, LinkElementData linkData2)
    {
        var transform1 = linkData1.LinkInstance.GetTotalTransform();
        var transform2 = linkData2.LinkInstance.GetTotalTransform();
        var transform2To1 = transform1.Inverse.Multiply(transform2);
        return (transform1, transform2, transform2To1);
    }

    private void ProcessElementPair(
        Element elem1, Solid solid1,
        Element elem2, Solid solid2,
        Transform transform2To1, Transform transform1,
        List<ClashResult> clashResults)
    {
        var transformedSolid2 = SolidUtils.CreateTransformed(solid2, transform2To1);

        try
        {
            var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                solid1, transformedSolid2, BooleanOperationsType.Intersect);

            if (intersection != null && intersection.Volume > MinVolumeTolerance)
                AddClashResultIfValid(intersection, elem1, elem2, transform1, clashResults);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar par de elementos (ID: {elem1.Id}, ID: {elem2.Id}): {ex.Message}");
        }
    }

    private void AddClashResultIfValid(
        Solid intersection,
        Element elem1, Element elem2,
        Transform transform1,
        List<ClashResult> clashResults)
    {
        var topFace = GetTopFace(intersection);
        if (topFace == null) return;

        var bboxUV = topFace.GetBoundingBox();
        var centerUV = (bboxUV.Min + bboxUV.Max) / 2.0;
        var localCenterPoint = topFace.Evaluate(centerUV);
        var worldCenterPoint = transform1.OfPoint(localCenterPoint);

        var (thickness, diameter) = GetClashParameters(elem1, elem2);

        if (thickness > MinClashParameterValue || diameter > MinClashParameterValue)
            clashResults.Add(new ClashResult(worldCenterPoint, elem1, elem2, intersection.Volume,
                thickness, diameter));
    }

    private (double thickness, double diameter) GetClashParameters(Element elem1, Element elem2)
    {
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

        return (thickness, diameter);
    }

    private Solid GetSolidFromElement(Element element)
    {
        var options = new Options { ComputeReferences = true, DetailLevel = ViewDetailLevel.Fine };
        var geomElem = element.get_Geometry(options);
        if (geomElem == null) return null;

        foreach (var geomObj in geomElem)
        {
            if (geomObj is Solid solid && solid.Volume > MinVolumeTolerance) return solid;
            if (geomObj is GeometryInstance geomInst)
                foreach (var nestedGeomObj in geomInst.GetInstanceGeometry())
                    if (nestedGeomObj is Solid nestedSolid && nestedSolid.Volume > MinVolumeTolerance)
                        return nestedSolid;
        }

        return null;
    }

    private PlanarFace GetTopFace(Solid solid)
    {
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
}