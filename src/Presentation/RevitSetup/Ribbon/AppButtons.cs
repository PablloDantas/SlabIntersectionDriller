namespace ClashOpenings.src.Presentation.RevitSetup.Ribbon;

/// <summary>
///     Classe estática que define as propriedades e configurações para os botões da interface do usuário da aplicação.
///     Novos botões devem ser declarados aqui utilizando a estrutura <see cref="ButtonData" />.
/// </summary>
public static class AppButtons
{
    /// <summary>
    ///     Define as propriedades do botão "Inserir Passagens".
    ///     Este botão detecta conflitos entre elementos e lajes, e automatiza a inserção de furos.
    /// </summary>
    public static readonly ButtonData SlabsOpeningsButton = new(
        "SlabsOpenings",
        "Inserir Passagens",
        "ClashOpenings.src.Commands.SlabsOpeningsCommand",
        "Detecta conflitos entre elementos e lajes, e instancia famílias de furos automaticamente no local da interferência.",
        "SlabsOpenings.png"
    );

    /// <summary>
    ///     Uma lista que contém todos os objetos <see cref="ButtonData" /> definidos na aplicação.
    ///     Facilita a iteração para a construção da interface do Ribbon.
    /// </summary>
    public static readonly List<ButtonData> AllButtons = new()
    {
        SlabsOpeningsButton
    };

    /// <summary>
    ///     Um dicionário para organizar botões em grupos específicos dentro do Ribbon.
    ///     A chave representa o nome do grupo e o valor é uma lista de <see cref="ButtonData" /> pertencentes a esse grupo.
    /// </summary>
    public static readonly Dictionary<string, List<ButtonData>> ButtonGroups = new()
    {
        {
            "ClashPanelControls", []
        }
    };
}