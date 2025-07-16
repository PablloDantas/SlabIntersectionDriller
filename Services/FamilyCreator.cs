using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using ClashOpenings.Models;

namespace ClashOpenings.Services;

/// <summary>
///     Gerencia a criação de instâncias de famílias de furações no documento do Revit
///     com base nos resultados da detecção de conflitos.
/// </summary>
public class FamilyCreator
{
    // Constantes para deslocamentos e folgas, convertidas para unidades internas do Revit.
    private const double ThicknessClearanceCentimeters = 10;
    private const double DiameterClearanceCentimeters = 1;

    private static readonly double ThicknessClearanceInternal =
        UnitUtils.ConvertToInternalUnits(ThicknessClearanceCentimeters, UnitTypeId.Centimeters);

    private static readonly double DiameterClearanceInternal =
        UnitUtils.ConvertToInternalUnits(DiameterClearanceCentimeters, UnitTypeId.Centimeters);

    private readonly Document _doc;

    /// <summary>
    ///     Inicializa uma nova instância da classe <see cref="FamilyCreator" />.
    /// </summary>
    /// <param name="doc">O documento ativo do Revit onde as furações serão criadas.</param>
    public FamilyCreator(Document doc)
    {
        _doc = doc;
    }

    /// <summary>
    ///     Cria furações no documento do Revit com base nos resultados de conflitos fornecidos.
    ///     Procura uma família de furação específica e, se encontrada, cria uma instância
    ///     para cada resultado de conflito, definindo seus parâmetros.
    /// </summary>
    /// <param name="clashResults">Uma lista de objetos <see cref="ClashResult" /> que representam os conflitos encontrados.</param>
    /// <returns>O número de furações criadas com sucesso.</returns>
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
        using (var t = new Transaction(_doc, "Criar Furações em Lajes"))
        {
            t.Start();

            foreach (var clash in clashResults)
                if (TryCreateSingleOpening(clash, familySymbol))
                    createdCount++;

            t.Commit();
        }

        return createdCount;
    }

    /// <summary>
    ///     Encontra e ativa o FamilySymbol específico para a criação de furações em lajes
    ///     ("FURO-QUADRADO-LAJE" com tipo "SDR - Furo na laje").
    /// </summary>
    /// <returns>O <see cref="FamilySymbol" /> se encontrado e ativado, caso contrário, <see langword="null" />.</returns>
    private FamilySymbol? FindAndActivateOpeningFamilySymbol()
    {
        var familySymbol = new FilteredElementCollector(_doc)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .FirstOrDefault(fs =>
                fs.Family.Name == RevitFamilyConstants.FamilyName && fs.Name == RevitFamilyConstants.FamilyTypeName);

        if (familySymbol != null && !familySymbol.IsActive)
        {
            familySymbol.Activate();
            _doc.Regenerate();
        }

        return familySymbol;
    }

    /// <summary>
    ///     Tenta criar uma única instância da família de furação em um ponto de inserção derivado
    ///     do resultado do conflito.
    /// </summary>
    /// <param name="clash">O resultado do conflito usado para determinar a localização e os parâmetros da furação.</param>
    /// <param name="familySymbol">O <see cref="FamilySymbol" /> da furação a ser instanciada.</param>
    /// <returns><see langword="true" /> se a furação foi criada com sucesso; caso contrário, <see langword="false" />.</returns>
    private bool TryCreateSingleOpening(ClashResult clash, FamilySymbol familySymbol)
    {
        var insertionPoint = clash.CenterPoint.Add(new XYZ(0, 0, ThicknessClearanceInternal / 2));

        var instance = _doc.Create.NewFamilyInstance(insertionPoint, familySymbol, StructuralType.NonStructural);

        if (instance == null) return false;

        SetOpeningParameters(instance, clash);
        return true;
    }

    /// <summary>
    ///     Define os parâmetros específicos da instância da família de furação (espessura e diâmetro)
    ///     com base nos dados do resultado do conflito.
    /// </summary>
    /// <param name="instance">A instância da família de furação recém-criada.</param>
    /// <param name="clash">O <see cref="ClashResult" /> contendo os dados do conflito.</param>
    private static void SetOpeningParameters(FamilyInstance instance, ClashResult clash)
    {
        if (clash.Thickness > 0) SetThicknessParameters(instance, clash);

        if (clash.Diameter > 0) SetDiameterParameters(instance, clash);
    }

    /// <summary>
    ///     Define os parâmetros relacionados à espessura da furação e o ID do elemento de piso
    ///     na instância da família.
    /// </summary>
    /// <param name="instance">A instância da família de furação.</param>
    /// <param name="clash">O resultado do conflito.</param>
    private static void SetThicknessParameters(FamilyInstance instance, ClashResult clash)
    {
        var finalThickness = clash.Thickness + ThicknessClearanceInternal;

        var thicknessParam = instance.LookupParameter(RevitFamilyConstants.ThicknessParameterName);
        thicknessParam?.Set(finalThickness);

        var floorIdParam = instance.LookupParameter(RevitFamilyConstants.FloorIdParameterName);
        floorIdParam?.Set(clash.GetFloor()?.Id.Value ?? ElementId.InvalidElementId.Value);
    }

    /// <summary>
    ///     Define os parâmetros relacionados ao diâmetro da furação e o ID do elemento MEP (tubulação/conduíte)
    ///     na instância da família.
    /// </summary>
    /// <param name="instance">A instância da família de furação.</param>
    /// <param name="clash">O resultado do conflito.</param>
    private static void SetDiameterParameters(FamilyInstance instance, ClashResult clash)
    {
        var finalDiameter = clash.Diameter + DiameterClearanceInternal;

        var dim1Param = instance.LookupParameter(RevitFamilyConstants.Dimension1ParameterName);
        dim1Param?.Set(finalDiameter);
        var dim2Param = instance.LookupParameter(RevitFamilyConstants.Dimension2ParameterName);
        dim2Param?.Set(finalDiameter);

        var curveIdParam = instance.LookupParameter(RevitFamilyConstants.CurveIdParameterName);
        curveIdParam?.Set(clash.GetMepCurve()?.Id.Value ?? ElementId.InvalidElementId.Value);
    }
}