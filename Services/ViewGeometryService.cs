using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ClashOpenings.Services;

/// <summary>
///     Fornece funcionalidades para determinar o volume de busca geométrica com base na vista ativa do Revit.
///     Suporta vistas de planta e vistas 3D com caixa de corte ativa.
/// </summary>
public class ViewGeometryService
{
    private readonly View _activeView;
    private readonly double _basePointElevation;
    private readonly Document _doc;

    /// <summary>
    ///     Inicializa uma nova instância da classe <see cref="ViewGeometryService" />.
    /// </summary>
    /// <param name="doc">O documento ativo do Revit.</param>
    public ViewGeometryService(Document doc)
    {
        _doc = doc;
        _activeView = doc.ActiveView;
        _basePointElevation = GetBasePointElevation();
    }

    /// <summary>
    ///     Obtém o volume de busca (representado por um <see cref="Outline" />) com base na vista ativa.
    ///     Se a vista ativa for uma <see cref="ViewPlan" />, o volume é derivado da região de corte e do alcance da vista.
    ///     Se for uma <see cref="View3D" /> com uma caixa de seção ativa, o volume é derivado da caixa de seção.
    /// </summary>
    /// <returns>
    ///     Um objeto <see cref="Outline" /> que define o volume de busca, ou <see langword="null" />
    ///     se a vista ativa não for suportada.
    /// </returns>
    public Outline GetSearchVolume()
    {
        if (_activeView is ViewPlan viewPlan) return GetSearchVolumeForViewPlan(viewPlan);

        if (_activeView is View3D view3D && view3D.IsSectionBoxActive) return GetSearchVolumeForView3D(view3D);

        TaskDialog.Show("Erro",
            "Tipo de vista não suportado. Use uma vista de planta ou uma vista 3D com uma caixa de seção ativa.");
        return null;
    }

    /// <summary>
    ///     Calcula o volume de busca para uma vista de planta (<see cref="ViewPlan" />).
    ///     O volume é determinado pela região de corte da vista e pelo seu alcance de vista (View Range).
    /// </summary>
    /// <param name="viewPlan">A vista de planta para a qual o volume de busca será calculado.</param>
    /// <returns>Um objeto <see cref="Outline" /> que define o volume de busca para a vista de planta.</returns>
    private Outline GetSearchVolumeForViewPlan(ViewPlan viewPlan)
    {
        var cropBox = viewPlan.CropBox;
        var vr = viewPlan.GetViewRange();
        var topZ = GetPlaneZ(_doc, vr, PlanViewPlane.TopClipPlane, _basePointElevation);
        var bottomZ = GetPlaneZ(_doc, vr, PlanViewPlane.BottomClipPlane, _basePointElevation);
        var newMin = new XYZ(cropBox.Min.X, cropBox.Min.Y, bottomZ);
        var newMax = new XYZ(cropBox.Max.X, cropBox.Max.Y, topZ);
        return new Outline(newMin, newMax);
    }

    /// <summary>
    ///     Calcula o volume de busca para uma vista 3D (<see cref="View3D" />) com uma caixa de seção ativa.
    ///     O volume é derivado da caixa de seção da vista.
    /// </summary>
    /// <param name="view3D">A vista 3D com caixa de seção ativa para a qual o volume de busca será calculado.</param>
    /// <returns>Um objeto <see cref="Outline" /> que define o volume de busca para a vista 3D.</returns>
    private Outline GetSearchVolumeForView3D(View3D view3D)
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
        return new Outline(new XYZ(xMin, yMin, zMin), new XYZ(xMax, yMax, zMax));
    }

    /// <summary>
    ///     Obtém a elevação do ponto base do projeto no documento.
    /// </summary>
    /// <returns>A elevação do ponto base do projeto em unidades internas do Revit, ou 0 se não for encontrada.</returns>
    private double GetBasePointElevation()
    {
        var basePoints = new FilteredElementCollector(_doc)
            .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
            .WhereElementIsNotElementType()
            .ToElements();

        foreach (var bp in basePoints)
        {
            var param = bp.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
            if (param != null && param.HasValue) return param.AsDouble();
        }

        return 0;
    }

    /// <summary>
    ///     Calcula a coordenada Z de um plano específico dentro do alcance de vista de uma planta,
    ///     ajustado pela elevação do ponto base do projeto.
    /// </summary>
    /// <param name="doc">O documento do Revit.</param>
    /// <param name="vr">O objeto <see cref="PlanViewRange" /> da vista de planta.</param>
    /// <param name="plane">O <see cref="PlanViewPlane" /> para o qual a coordenada Z será calculada.</param>
    /// <param name="bpElevation">A elevação do ponto base do projeto para ajuste.</param>
    /// <returns>A coordenada Z do plano em unidades internas do Revit.</returns>
    private double GetPlaneZ(Document doc, PlanViewRange vr, PlanViewPlane plane, double bpElevation)
    {
        try
        {
            var level = doc.GetElement(vr.GetLevelId(plane)) as Level;
            return level.Elevation + vr.GetOffset(plane) - bpElevation;
        }
        catch
        {
            // Fallback: se o plano superior/inferior não estiver associado a um nível,
            // usa o nível do plano de corte como referência.
            var cutLevel = doc.GetElement(vr.GetLevelId(PlanViewPlane.CutPlane)) as Level;
            return cutLevel.Elevation + vr.GetOffset(PlanViewPlane.CutPlane) - bpElevation;
        }
    }
}