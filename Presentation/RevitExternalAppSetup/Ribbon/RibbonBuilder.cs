using System.Reflection;
using Autodesk.Revit.UI;
using ClashOpenings.Presentation.Helpers;

namespace ClashOpenings.Presentation.RevitExternalAppSetup.Ribbon;

/// <summary>
///     Responsável por construir a aba, painel e botões da aplicação na interface do Revit.
/// </summary>
public class RibbonBuilder
{
    private const string TabName = "Clash Openings"; // Nome da sua aba personalizada
    private const string PanelName = "Openings Instance"; // Nome do painel
    private readonly UIControlledApplication _application;

    public RibbonBuilder(UIControlledApplication application)
    {
        _application = application;
    }

    public void BuildRibbon()
    {
        try
        {
            _application.CreateRibbonTab(TabName);
        }
        catch (Exception)
        {
            // A aba já existe, podemos ignorar este erro
        }

        var ribbonPanel = GetOrCreateRibbonPanel(TabName, PanelName);

        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        // Adicionar botões individuais
        foreach (var buttonData in AppButtons.AllButtons)
        {
            // Pular botões que estão em grupos
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

        // Adicionar grupos de botões
        foreach (var group in AppButtons.ButtonGroups)
        {
            if (group.Value.Count == 0)
                continue;

            // Criamos os botões
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

            // Agora adicionamos os botões de acordo com a quantidade
            if (buttons.Count == 2)
                ribbonPanel.AddStackedItems(buttons[0], buttons[1]);
            else if (buttons.Count == 3)
                ribbonPanel.AddStackedItems(buttons[0], buttons[1], buttons[2]);
            else if (buttons.Count > 0)
                // Para mais de 3 botões, criamos subpainéis
                // Ou adicione cada um individualmente
                foreach (var button in buttons)
                    ribbonPanel.AddItem(button);
        }
    }

    private RibbonPanel GetOrCreateRibbonPanel(string tabName, string panelName)
    {
        // Tenta encontrar o painel existente
        foreach (var panel in _application.GetRibbonPanels(tabName))
            if (panel.Name == panelName)
                return panel;

        // Se não encontrar, cria um novo
        return _application.CreateRibbonPanel(tabName, panelName);
    }
}