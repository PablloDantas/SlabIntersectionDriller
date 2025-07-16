using Autodesk.Revit.UI;
using ClashOpenings.src.Presentation.RevitSetup.Ribbon;
using ClashOpenings.src.Presentation.Vendors.Ricaun;
using ClashOpenings.src.Presentation.Views;

namespace ClashOpenings.src.Presentation.RevitSetup;

/// <summary>
///     Classe principal da aplicação que implementa <see cref="IExternalApplication" />,
///     servindo como ponto de entrada para inicialização e desligamento no ambiente Revit.
/// </summary>
public class ClashOpeningsApp : IExternalApplication
{
    /// <summary>
    ///     Serviço responsável por gerenciar a criação e registro de painéis ancoráveis no Revit.
    /// </summary>
    public static DockablePaneCreatorService? DockablePaneCreatorService { get; private set; }

    /// <summary>
    ///     A instância da interface de usuário para a seleção de aberturas em lajes.
    /// </summary>
    public static ClashSelectionView? SlabsOpeningsPane { get; private set; }

    /// <summary>
    ///     O identificador único (GUID) para o painel ancorável de seleção de aberturas em lajes.
    /// </summary>
    public static Guid SlabsOpeningsGuid { get; private set; }

    /// <summary>
    ///     Método chamado pelo Revit quando a aplicação é iniciada.
    ///     Configura os serviços, painéis ancoráveis e a Ribbon da aplicação.
    /// </summary>
    /// <param name="application">A aplicação UI controlada pelo Revit.</param>
    /// <returns>O resultado da operação de inicialização.</returns>
    public Result OnStartup(UIControlledApplication application)
    {
        DockablePaneCreatorService = new DockablePaneCreatorService(application);
        DockablePaneCreatorService.Initialize();

        SlabsOpeningsGuid = Guid.NewGuid();
        SlabsOpeningsPane = new ClashSelectionView();

        application.ControlledApplication.ApplicationInitialized += (_, _) =>
        {
            DockablePaneCreatorService.Register(
                SlabsOpeningsGuid,
                "Slabs Openings",
                SlabsOpeningsPane);
        };

        var ribbonBuilder = new RibbonBuilder(application);
        ribbonBuilder.BuildRibbon();

        return Result.Succeeded;
    }

    /// <summary>
    ///     Método chamado pelo Revit quando a aplicação é desligada.
    ///     Realiza a limpeza e liberação de recursos.
    /// </summary>
    /// <param name="application">A aplicação UI controlada pelo Revit.</param>
    /// <returns>O resultado da operação de desligamento.</returns>
    public Result OnShutdown(UIControlledApplication application)
    {
        DockablePaneCreatorService?.Dispose();

        return Result.Succeeded;
    }
}