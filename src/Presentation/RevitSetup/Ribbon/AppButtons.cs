namespace ClashOpenings.src.Presentation.RevitSetup.Ribbon;

/// <summary>
///     Classe estática que armazena as definições para todos os botões da aplicação.
///     Para adicionar um novo botão, basta criar uma nova instância de ButtonData aqui.
/// </summary>
public static class AppButtons
{
    // Defina aqui cada botão da sua aplicação
    public static readonly ButtonData SlabsOpeningsButton = new(
        "SlabsOpenings",
        "Inserir Passagens",
        "ClashOpenings.src.Commands.SlabsOpeningsCommand", // Exemplo de namespace do comando
        "Detecta conflitos entre elementos e lajes, e instancia famílias de furos automaticamente no local da interferência.",
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