using Autodesk.Revit.DB;

namespace ClashOpenings.src.Models;

/// <summary>
///     Representa o resultado de uma colisão detectada entre dois elementos no Revit,
///     incluindo informações sobre o ponto de interseção, volume, espessura e diâmetro.
/// </summary>
public class ClashResult
{
    /// <summary>
    ///     Inicializa uma nova instância da classe <see cref="ClashResult" />.
    /// </summary>
    /// <param name="centerPoint">O ponto central da interseção entre os elementos.</param>
    /// <param name="element1">O primeiro elemento envolvido na colisão.</param>
    /// <param name="element2">O segundo elemento envolvido na colisão.</param>
    /// <param name="intersectionVolume">O volume da interseção entre os elementos.</param>
    /// <param name="thickness">A espessura calculada para a abertura (por exemplo, espessura da laje).</param>
    /// <param name="diameter">O diâmetro calculado para a abertura (por exemplo, diâmetro da tubulação).</param>
    public ClashResult(
        XYZ centerPoint,
        Element element1,
        Element element2,
        double intersectionVolume,
        double thickness,
        double diameter)
    {
        CenterPoint = centerPoint;
        Element1 = element1;
        Element2 = element2;
        IntersectionVolume = intersectionVolume;
        Thickness = thickness;
        Diameter = diameter;
    }

    /// <summary>
    ///     Obtém o ponto central da interseção dos elementos.
    /// </summary>
    public XYZ CenterPoint { get; }

    /// <summary>
    ///     Obtém o primeiro elemento envolvido na colisão.
    /// </summary>
    public Element Element1 { get; }

    /// <summary>
    ///     Obtém o segundo elemento envolvido na colisão.
    /// </summary>
    public Element Element2 { get; }

    /// <summary>
    ///     Obtém o volume da interseção entre os elementos.
    /// </summary>
    public double IntersectionVolume { get; }

    /// <summary>
    ///     Obtém a espessura calculada para a abertura.
    /// </summary>
    public double Thickness { get; }

    /// <summary>
    ///     Obtém o diâmetro calculado para a abertura.
    /// </summary>
    public double Diameter { get; }

    /// <summary>
    ///     Tenta obter um elemento do tipo <see cref="Floor" /> (laje) a partir dos elementos da colisão.
    /// </summary>
    /// <returns>
    ///     O objeto <see cref="Floor" /> se um dos elementos for uma laje; caso contrário, <see langword="null" />.
    /// </returns>
    public Floor? GetFloor()
    {
        return Element1 as Floor ?? Element2 as Floor;
    }

    /// <summary>
    ///     Tenta obter um elemento do tipo <see cref="MEPCurve" /> (curva MEP, como tubulação ou duto)
    ///     a partir dos elementos da colisão.
    /// </summary>
    /// <returns>
    ///     O objeto <see cref="MEPCurve" /> se um dos elementos for uma curva MEP; caso contrário, <see langword="null" />.
    /// </returns>
    public MEPCurve? GetMepCurve()
    {
        return Element1 as MEPCurve ?? Element2 as MEPCurve;
    }
}