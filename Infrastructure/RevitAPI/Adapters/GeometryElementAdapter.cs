using Autodesk.Revit.DB;
using SolidDomain = ClashOpenings.Core.Domain.ValueObjects.Geometries.Solid;
using RevitSolid = Autodesk.Revit.DB.Solid;

namespace ClashOpenings.Infrastructure.RevitAPI.Adapters;

public static class GeometryElementAdapter
{
    public static SolidDomain ToDomainSolid(this GeometryElement geometryElement)
    {
        var vertices = new List<float>();
        var indices = new List<int>();
        var vertexMap = new Dictionary<string, int>();
        var vertexCounter = 0;

        foreach (var geometryObject in geometryElement)
        {
            if (geometryObject is not RevitSolid solid) continue;
            if (solid.Volume.Equals(0)) continue;

            foreach (Face face in solid.Faces)
            {
                var mesh = face.Triangulate();
                for (var i = 0; i < mesh.NumTriangles; i++)
                {
                    var triangle = mesh.get_Triangle(i);

                    for (var j = 0; j < 3; j++)
                    {
                        var vertex = triangle.get_Vertex(j);
                        var vertexKey = $"{vertex.X:F6},{vertex.Y:F6},{vertex.Z:F6}";

                        if (!vertexMap.TryGetValue(vertexKey, out var index))
                        {
                            index = vertexCounter++;
                            vertexMap.Add(vertexKey, index);
                            vertices.Add((float)vertex.X);
                            vertices.Add((float)vertex.Y);
                            vertices.Add((float)vertex.Z);
                        }

                        indices.Add(index);
                    }
                }
            }
        }

        return SolidDomain.Create(vertices.ToArray(), indices.ToArray());
    }
}