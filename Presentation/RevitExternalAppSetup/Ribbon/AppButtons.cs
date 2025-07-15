using ClashOpenings.Presentation.RevitExternalAppSetup.Entities.Data;

namespace ClashOpenings.Presentation.RevitExternalAppSetup.Ribbon;

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

    // Botões para o painel ancorado de detecção de conflitos
    public static readonly ButtonData RegisterClashPanelButton = new(
        "RegisterClashPanel",
        "Registrar Painel",
        "ClashOpenings.Presentation.Commands.RegisterClashSelectionPaneCommand",
        "Registra o painel ancorado para detecção de conflitos",
        "RegisterPanel.png"
    );

    public static readonly ButtonData ShowClashPanelButton = new(
        "ShowClashPanel",
        "Mostrar Painel",
        "ClashOpenings.Presentation.Commands.ShowClashSelectionPaneCommand",
        "Mostra o painel ancorado para detecção de conflitos",
        "ShowPanel.png"
    );

    public static readonly ButtonData HideClashPanelButton = new(
        "HideClashPanel",
        "Ocultar Painel",
        "ClashOpenings.Presentation.Commands.HideClashSelectionPaneCommand",
        "Oculta o painel ancorado para detecção de conflitos",
        "HidePanel.png"
    );

    // Uma lista para facilitar a iteração pelo RibbonBuilder
    public static readonly List<ButtonData> AllButtons = new()
    {
        SlabsOpeningsButton,
        RegisterClashPanelButton,
        ShowClashPanelButton,
        HideClashPanelButton
    };

    // Lista específica para botões que devem ficar agrupados
    public static readonly Dictionary<string, List<ButtonData>> ButtonGroups = new()
    {
        {
            "ClashPanelControls", new List<ButtonData>
            {
                RegisterClashPanelButton,
                ShowClashPanelButton,
                HideClashPanelButton
            }
        }
    };
}