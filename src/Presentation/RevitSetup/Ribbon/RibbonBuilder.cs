using System.Reflection;
using Autodesk.Revit.UI;
using ClashOpenings.src.Helpers;

namespace ClashOpenings.src.Presentation.RevitSetup.Ribbon;

/// <summary>
///     Gerencia a construção e configuração da aba (tab), painel e botões personalizados
///     da aplicação na interface de usuário do Revit (Ribbon).
/// </summary>
public class RibbonBuilder
{
    /// <summary>
    ///     O nome da aba personalizada que será criada no Ribbon do Revit.
    /// </summary>
    private const string TabName = "Passagens BIM";

    /// <summary>
    ///     O nome do painel que será criado dentro da aba personalizada.
    /// </summary>
    private const string PanelName = "Lajes e Pisos";

    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Inicializa uma nova instância da classe <see cref="RibbonBuilder" />.
    /// </summary>
    /// <param name="application">A instância da aplicação controlada pelo Revit UI.</param>
    public RibbonBuilder(UIControlledApplication application)
    {
        _application = application;
    }

    /// <summary>
    ///     Constrói a aba, o painel e adiciona todos os botões definidos em <see cref="AppButtons" />
    ///     ao Ribbon do Revit.
    /// </summary>
    public void BuildRibbon()
    {
        try
        {
            _application.CreateRibbonTab(TabName);
        }
        catch (Exception)
        {
            // A aba pode já existir; ignora a exceção.
        }

        var ribbonPanel = GetOrCreateRibbonPanel(TabName, PanelName);

        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        // Adiciona botões individuais que não pertencem a grupos.
        foreach (var buttonData in AppButtons.AllButtons)
        {
            if (AppButtons.ButtonGroups.Values.Any(group => group.Contains(buttonData)))
                continue;

            var pushButtonData = new PushButtonData(
                buttonData.Name,
                buttonData.Text,
                assemblyPath,
                buttonData.CommandNamespace
            );

            pushButtonData.ToolTip = buttonData.Tooltip;
            pushButtonData.Image = ImageHelper.LoadImageSource(buttonData.IconName);
            pushButtonData.LargeImage = ImageHelper.LoadImageSource(buttonData.IconName);

            ribbonPanel.AddItem(pushButtonData);
        }

        // Adiciona grupos de botões ao painel.
        foreach (var group in AppButtons.ButtonGroups)
        {
            if (group.Value.Count == 0)
                continue;

            var buttons = new List<PushButtonData>();
            foreach (var buttonData in group.Value)
            {
                var pushButtonData = new PushButtonData(
                    buttonData.Name,
                    buttonData.Text,
                    assemblyPath,
                    buttonData.CommandNamespace
                );

                pushButtonData.ToolTip = buttonData.Tooltip;
                pushButtonData.Image = ImageHelper.LoadImageSource(buttonData.IconName);
                pushButtonData.LargeImage = ImageHelper.LoadImageSource(buttonData.IconName);

                buttons.Add(pushButtonData);
            }

            // Organiza os botões em pilhas ou individualmente com base na contagem.
            if (buttons.Count == 2)
                ribbonPanel.AddStackedItems(buttons[0], buttons[1]);
            else if (buttons.Count == 3)
                ribbonPanel.AddStackedItems(buttons[0], buttons[1], buttons[2]);
            else if (buttons.Count > 0)
                foreach (var button in buttons)
                    ribbonPanel.AddItem(button);
        }
    }

    /// <summary>
    ///     Obtém um painel do Ribbon existente ou cria um novo se não for encontrado.
    /// </summary>
    /// <param name="tabName">O nome da aba onde o painel está localizado ou será criado.</param>
    /// <param name="panelName">O nome do painel a ser procurado ou criado.</param>
    /// <returns>Uma instância de <see cref="RibbonPanel" />.</returns>
    private RibbonPanel GetOrCreateRibbonPanel(string tabName, string panelName)
    {
        // Tenta encontrar o painel existente na aba especificada.
        foreach (var panel in _application.GetRibbonPanels(tabName))
            if (panel.Name == panelName)
                return panel;

        // Se o painel não for encontrado, cria um novo.
        return _application.CreateRibbonPanel(tabName, panelName);
    }
}