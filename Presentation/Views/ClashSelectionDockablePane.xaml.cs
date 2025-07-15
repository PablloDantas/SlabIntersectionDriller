using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using ClashOpenings.Presentation.ViewModels;

namespace ClashOpenings.Presentation.Views;

public partial class ClashSelectionDockablePane : UserControl, IDockablePaneProvider
{
    // Armazenar o UIDocument atual para uso posterior
    private UIDocument _currentUiDoc;

    public ClashSelectionDockablePane()
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

        // Se for um ClashSelectionViewModel, armazenar a referência ao UIDocument
        if (viewModel is ClashSelectionViewModel vm && vm.GetType().GetField("_uiDoc",
                    BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(vm)
                is UIDocument uiDoc)
            _currentUiDoc = uiDoc;
    }

    private void RunClashButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ClashSelectionViewModel viewModel)
        {
            // Verifica se ambos os links estão selecionados
            if (viewModel.SelectedLinkInstance1 != null &&
                viewModel.SelectedLinkInstance2 != null &&
                viewModel.SelectedLinkInstance1.Id != viewModel.SelectedLinkInstance2.Id)
            {
                // O comando RunClashDetectionCommand já implementa a lógica necessária
                // usando ExternalEvent, então apenas execute-o
                if (viewModel.RunClashDetectionCommand.CanExecute(null))
                    viewModel.RunClashDetectionCommand.Execute(null);
            }
            else
            {
                TaskDialog.Show("Selection Required", "Please select two different models.");
                viewModel.StatusMessage = "Please select two different models.";
            }
        }
    }
}