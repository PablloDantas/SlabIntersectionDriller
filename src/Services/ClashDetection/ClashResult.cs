using Autodesk.Revit.DB;

namespace ClashOpenings.src.Services.ClashDetection
{
    public class ClashResult
    {
        public XYZ CenterPoint { get; }
        public Element Element1 { get; }
        public Element Element2 { get; }
        public double IntersectionVolume { get; }

        public ClashResult(XYZ centerPoint, Element element1, Element element2, double intersectionVolume)
        {
            CenterPoint = centerPoint;
            Element1 = element1;
            Element2 = element2;
            IntersectionVolume = intersectionVolume;
        }
    }
}
