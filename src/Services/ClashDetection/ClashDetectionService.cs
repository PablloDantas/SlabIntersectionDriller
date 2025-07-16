using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ClashOpenings.src.Services.ClashDetection
{
    public class ClashDetectionService
    {
        public List<ClashResult> FindClashes(
            (RevitLinkInstance, List<Element>) linkData1,
            (RevitLinkInstance, List<Element>) linkData2)
        {
            var clashResults = new List<ClashResult>();
            var (linkInst1, elements1) = linkData1;
            var (linkInst2, elements2) = linkData2;

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

                            clashResults.Add(new ClashResult(worldCenterPoint, elem1, elem2, intersection.Volume));
                        }
                    }
                    catch
                    {
                        // Ignore boolean operation failures
                    }
                }
            }
            return clashResults;
        }

        public Dictionary<XYZ, (double thickness, double diameter)> ProcessClashResults(List<ClashResult> clashResults)
        {
            var clashInformation = new Dictionary<XYZ, (double thickness, double diameter)>();
            foreach (var clash in clashResults)
            {
                var elem1 = clash.Element1;
                var elem2 = clash.Element2;

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
                    clashInformation[clash.CenterPoint] = (thickness, diameter);
            }
            return clashInformation;
        }

        private Solid GetSolidFromElement(Element element)
        {
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

        private PlanarFace GetTopFace(Solid solid)
        {
            PlanarFace topFace = null;
            var highestZ = double.MinValue;

            foreach (Face face in solid.Faces)
            {
                if (face is PlanarFace planarFace && planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ))
                {
                    if (planarFace.Origin.Z > highestZ)
                    {
                        highestZ = planarFace.Origin.Z;
                        topFace = planarFace;
                    }
                }
            }
            return topFace;
        }
    }
}
