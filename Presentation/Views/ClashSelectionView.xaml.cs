using Autodesk.Revit.UI;

namespace ClashOpenings.Presentation.Views;

/// <summary>
///     Representa a interface de usuário para seleção de modelos na detecção de conflitos de aberturas.
///     Implementa <see cref="IDockablePaneProvider" /> para integração como um painel ancorável no Revit.
/// </summary>
public partial class ClashSelectionView : IDockablePaneProvider
{
    /// <summary>
    ///     Inicializa uma nova instância da classe <see cref="ClashSelectionView" />.
    /// </summary>
    public ClashSelectionView()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Configura os dados do painel ancorável, incluindo o FrameworkElement, posição inicial e visibilidade.
    /// </summary>
    /// <param name="data">Os dados do provedor de painel ancorável a serem configurados.</param>
    public void SetupDockablePane(DockablePaneProviderData data)
    {
        data.FrameworkElement = this;
        data.InitialState = new DockablePaneState
        {
            DockPosition = DockPosition.Right,
            MinimumWidth = 500,
            MinimumHeight = 600
        };
        data.VisibleByDefault = false;
    }

    /// <summary>
    ///     Define o ViewModel como o DataContext para a interface do usuário.
    /// </summary>
    /// <param name="viewModel">O objeto ViewModel a ser definido como DataContext.</param>
    public void SetViewModel(object viewModel)
    {
        DataContext = viewModel;
    }
}