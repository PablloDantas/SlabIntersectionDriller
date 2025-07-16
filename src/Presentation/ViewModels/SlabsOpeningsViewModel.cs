using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.src.Services.Revit;

namespace ClashOpenings.src.Presentation.ViewModels;

public class SlabsOpeningsViewModel : INotifyPropertyChanged
{
    private ICommand _runClashDetectionCommand;
    private RevitLinkInstance? _selectedLinkInstance1;
    private RevitLinkInstance? _selectedLinkInstance2;
    private string _statusMessage;

    public SlabsOpeningsViewModel(UIDocument uiDoc)
    {
        var document = uiDoc.Document;

        LinkInstances = new ObservableCollection<RevitLinkInstance>(RevitElementCollector.GetAllLinkInstances(document));

        // Configurar o ExternalEvent
        var eventHandler = new SlabsOpeningsExternalEventHandler();
        eventHandler.SetStatusCallback(status => StatusMessage = status);
        var externalEvent = ExternalEvent.Create(eventHandler);

        _runClashDetectionCommand = new SlabsOpeningsRelayCommand(
            obj =>
            {
                try
                {
                    // Atualizar os links selecionados no handler
                    eventHandler.SetLinks(SelectedLinkInstance1, SelectedLinkInstance2);

                    // Disparar o evento externo
                    externalEvent.Raise();

                    StatusMessage = "Running clash detection...";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    TaskDialog.Show("Error", $"Failed to run clash detection: {ex.Message}");
                }
            },
            obj => SelectedLinkInstance1 != null &&
                   SelectedLinkInstance2 != null &&
                   SelectedLinkInstance1.Id != SelectedLinkInstance2.Id);
    }

    public ObservableCollection<RevitLinkInstance> LinkInstances { get; set; }

    public RevitLinkInstance? SelectedLinkInstance1
    {
        get => _selectedLinkInstance1;
        set
        {
            if (_selectedLinkInstance1 != value)
            {
                _selectedLinkInstance1 = value;
                OnPropertyChanged(nameof(SelectedLinkInstance1));
            }
        }
    }

    public RevitLinkInstance? SelectedLinkInstance2
    {
        get => _selectedLinkInstance2;
        set
        {
            if (_selectedLinkInstance2 != value)
            {
                _selectedLinkInstance2 = value;
                OnPropertyChanged(nameof(SelectedLinkInstance2));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    public ICommand RunClashDetectionCommand
    {
        get => _runClashDetectionCommand;
        set
        {
            if (_runClashDetectionCommand != value)
            {
                _runClashDetectionCommand = value;
                OnPropertyChanged(nameof(RunClashDetectionCommand));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}