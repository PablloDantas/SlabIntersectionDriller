namespace ClashOpenings.Models;

/// <summary>
///     Define constantes para nomes de famílias, tipos de família e parâmetros compartilhados
///     utilizados na criação e manipulação de aberturas no Revit.
/// </summary>
public static class RevitFamilyConstants
{
    /// <summary>
    ///     O nome da família Revit para furos em lajes.
    /// </summary>
    public const string FamilyName = "FURO-QUADRADO-LAJE";

    /// <summary>
    ///     O nome do tipo de família específico dentro da família de furos em lajes.
    /// </summary>
    public const string FamilyTypeName = "SDR - Furo na laje";

    /// <summary>
    ///     O nome do parâmetro compartilhado no Revit que define a espessura da laje para a abertura.
    /// </summary>
    public const string ThicknessParameterName = "FUR.esp-laje";

    /// <summary>
    ///     O nome do parâmetro compartilhado no Revit para armazenar o ID do elemento da laje associada.
    /// </summary>
    public const string FloorIdParameterName = "FUR.ESTRUTURA.ID";

    /// <summary>
    ///     O nome do parâmetro compartilhado no Revit para a primeira dimensão da abertura.
    /// </summary>
    public const string Dimension1ParameterName = "TH-FUR-DIM1";

    /// <summary>
    ///     O nome do parâmetro compartilhado no Revit para a segunda dimensão da abertura.
    /// </summary>
    public const string Dimension2ParameterName = "TH-FUR-DIM2";

    /// <summary>
    ///     O nome do parâmetro compartilhado no Revit para armazenar o ID da curva MEP (tubo/duto) associada.
    /// </summary>
    public const string CurveIdParameterName = "FUR.TUBO.ID";
}