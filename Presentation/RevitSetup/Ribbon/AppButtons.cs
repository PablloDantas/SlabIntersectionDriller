using ClashOpenings.Presentation.RevitSetup.Entities.Data;

namespace ClashOpenings.Presentation.RevitSetup.Ribbon;

/// <summary>
///     Classe estática que armazena as definições para todos os botões da aplicação.
///     Para adicionar um novo botão, basta criar uma nova instância de ButtonData aqui.
/// </summary>
public static class AppButtons
{
    // Defina aqui cada botão da sua aplicação
    public static readonly ButtonData SlabsOpeningsButton = new(
        "SlabsOpenings",
        "Slabs Openings",
        "ClashOpenings.Presentation.Commands.SlabsOpeningsCommand", // Exemplo de namespace do comando
        "Cria furos em lajes com base nas interseções de dutos.",
        "SlabsOpenings.png"
    );

    // Uma lista para facilitar a iteração pelo RibbonBuilder
    public static readonly List<ButtonData> AllButtons = new()
    {
        SlabsOpeningsButton
    };

    // Lista específica para botões que devem ficar agrupados
    public static readonly Dictionary<string, List<ButtonData>> ButtonGroups = new()
    {
        {
            "ClashPanelControls", []
        }
    };
}