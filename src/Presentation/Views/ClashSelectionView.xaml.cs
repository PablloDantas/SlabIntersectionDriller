using Autodesk.Revit.UI;

namespace ClashOpenings.src.Presentation.Views;

public partial class ClashSelectionView : IDockablePaneProvider
{
    public ClashSelectionView()
    {
        InitializeComponent();
    }

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

    public void SetViewModel(object viewModel)
    {
        DataContext = viewModel;
    }
}