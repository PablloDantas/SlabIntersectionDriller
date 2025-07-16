using Autodesk.Revit.DB;
using ClashOpenings.Models;

namespace ClashOpenings.Services;

/// <summary>
///     Fornece funcionalidades para detectar conflitos (clashes) entre elementos de dois modelos vinculados do Revit.
/// </summary>
public class ClashDetective
{
    private const double MinVolumeTolerance = 1e-9;
    private const double MinClashParameterValue = 0;

    /// <summary>
    ///     Encontra conflitos entre os elementos de dois conjuntos de dados de links do Revit.
    ///     Compara todos os elementos do primeiro link com todos os elementos do segundo link
    ///     para identificar sobreposições de geometria.
    /// </summary>
    /// <param name="linkData1">Dados dos elementos do primeiro modelo vinculado, incluindo sua instância.</param>
    /// <param name="linkData2">Dados dos elementos do segundo modelo vinculado, incluindo sua instância.</param>
    /// <returns>Uma lista de <see cref="ClashResult" /> detalhando cada conflito encontrado.</returns>
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

    /// <summary>
    ///     Calcula as transformações necessárias para alinhar os sólidos dos elementos dos modelos vinculados
    ///     no espaço coordenado do documento hospedeiro.
    /// </summary>
    /// <param name="linkData1">Dados do primeiro modelo vinculado.</param>
    /// <param name="linkData2">Dados do segundo modelo vinculado.</param>
    /// <returns>
    ///     Uma tupla contendo:
    ///     <list type="bullet">
    ///         <item><term>transform1</term>: A transformação total do primeiro modelo vinculado.</item>
    ///         <item><term>transform2</term>: A transformação total do segundo modelo vinculado.</item>
    ///         <item>
    ///             <term>transform2To1</term>: A transformação para converter geometria do link 2 para o sistema de
    ///             coordenadas do link 1.
    ///         </item>
    ///     </list>
    /// </returns>
    private (Transform transform1, Transform transform2, Transform transform2To1) CalculateClashTransforms(
        LinkElementData linkData1, LinkElementData linkData2)
    {
        var transform1 = linkData1.LinkInstance.GetTotalTransform();
        var transform2 = linkData2.LinkInstance.GetTotalTransform();
        var transform2To1 = transform1.Inverse.Multiply(transform2);
        return (transform1, transform2, transform2To1);
    }

    /// <summary>
    ///     Processa um par de elementos para verificar a ocorrência de um conflito.
    ///     Realiza uma operação booleana de interseção entre os sólidos transformados.
    /// </summary>
    /// <param name="elem1">O primeiro elemento envolvido no potencial conflito.</param>
    /// <param name="solid1">O sólido geométrico do primeiro elemento.</param>
    /// <param name="elem2">O segundo elemento envolvido no potencial conflito.</param>
    /// <param name="solid2">O sólido geométrico do segundo elemento.</param>
    /// <param name="transform2To1">A transformação para alinhar o sólido do segundo elemento com o primeiro.</param>
    /// <param name="transform1">A transformação do primeiro link, usada para determinar a localização mundial do conflito.</param>
    /// <param name="clashResults">A lista onde os resultados do conflito serão adicionados.</param>
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
            // Logar o erro ou lidar com ele de forma apropriada.
            // Console.WriteLine($"Erro ao processar par de elementos (ID: {elem1.Id}, ID: {elem2.Id}): {ex.Message}");
        }
    }

    /// <summary>
    ///     Adiciona um resultado de conflito à lista se o volume de interseção for significativo
    ///     e se parâmetros relevantes (espessura/diâmetro) puderem ser extraídos.
    /// </summary>
    /// <param name="intersection">O sólido resultante da interseção entre os elementos.</param>
    /// <param name="elem1">O primeiro elemento do conflito.</param>
    /// <param name="elem2">O segundo elemento do conflito.</param>
    /// <param name="transform1">A transformação do primeiro link para obter as coordenadas mundiais.</param>
    /// <param name="clashResults">A lista de resultados de conflito a ser preenchida.</param>
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

    /// <summary>
    ///     Obtém parâmetros de dimensão específicos (espessura para pisos, diâmetro para tubulações/conduítes)
    ///     de elementos envolvidos no conflito.
    /// </summary>
    /// <param name="elem1">O primeiro elemento.</param>
    /// <param name="elem2">O segundo elemento.</param>
    /// <returns>Uma tupla contendo a espessura e o diâmetro dos elementos, se aplicável.</returns>
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

    /// <summary>
    ///     Extrai o primeiro sólido geométrico com volume significativo de um elemento do Revit.
    /// </summary>
    /// <param name="element">O elemento do Revit do qual extrair o sólido.</param>
    /// <returns>Um objeto <see cref="Solid" /> representando a geometria do elemento, ou null se nenhum sólido for encontrado.</returns>
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

    /// <summary>
    ///     Localiza a face superior de um sólido geométrico.
    ///     A face superior é definida como a face planar com a normal apontando para cima (eixo Z)
    ///     e com o maior valor Z de origem.
    /// </summary>
    /// <param name="solid">O sólido do qual se deseja obter a face superior.</param>
    /// <returns>A <see cref="PlanarFace" /> que representa a face superior do sólido, ou null se não for encontrada.</returns>
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