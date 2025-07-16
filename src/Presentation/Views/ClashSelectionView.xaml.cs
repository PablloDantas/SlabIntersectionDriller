using System.Reflection;
using System.Windows;
using Autodesk.Revit.UI;
using ClashOpenings.src.Presentation.ViewModels;

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
            DockPosition = DockPosition.Right
        };
        data.VisibleByDefault = false;
    }

    public void SetViewModel(object viewModel)
    {
        DataContext = viewModel;
    }
}
