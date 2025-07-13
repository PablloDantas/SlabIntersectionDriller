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
        _application.CreateRibbonTab(TabName);
        var ribbonPanel = _application.CreateRibbonPanel(TabName, PanelName);

        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        foreach (var buttonData in AppButtons.AllButtons)
        {
            var pushButtonData = new PushButtonData(
                buttonData.Name,
                buttonData.Text,
                assemblyPath,
                buttonData.CommandNamespace
            );

            pushButtonData.ToolTip = buttonData.Tooltip;
            pushButtonData.Image = ImageHelper.LoadImageSource(buttonData.IconName);
            pushButtonData.LargeImage = ImageHelper.LoadImageSource(buttonData.IconName); // Pode usar ícones diferentes

            ribbonPanel.AddItem(pushButtonData);
        }
    }
}