using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.Presentation.Handlers;

namespace ClashOpenings.Presentation.ViewModels;

public class ClashSelectionViewModel : INotifyPropertyChanged
{
    private readonly UIDocument _uiDoc;

    // Adicione estes campos para o ExternalEvent
    private readonly ClashDetectionExternalEventHandler _eventHandler;
    private readonly ExternalEvent _externalEvent;
    private ICommand _runClashDetectionCommand;
    private RevitLinkInstance _selectedLinkInstance1;
    private RevitLinkInstance _selectedLinkInstance2;
    private string _statusMessage;

    public ClashSelectionViewModel(UIDocument uiDoc)
    {
        _uiDoc = uiDoc;
        var doc = uiDoc.Document;

        var linkInstances = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .ToList();
        LinkInstances = new ObservableCollection<RevitLinkInstance>(linkInstances);

        // Configurar o ExternalEvent
        _eventHandler = new ClashDetectionExternalEventHandler();
        _eventHandler.SetStatusCallback(status => StatusMessage = status);
        _externalEvent = ExternalEvent.Create(_eventHandler);

        _runClashDetectionCommand = new RelayCommand(
            obj =>
            {
                try
                {
                    // Atualizar os links selecionados no handler
                    _eventHandler.SetLinks(SelectedLinkInstance1, SelectedLinkInstance2);

                    // Disparar o evento externo
                    _externalEvent.Raise();

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

    public RevitLinkInstance SelectedLinkInstance1
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

    public RevitLinkInstance SelectedLinkInstance2
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

public class RelayCommand : ICommand
{
    private readonly Predicate<object> _canExecute;
    private readonly Action<object> _execute;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}