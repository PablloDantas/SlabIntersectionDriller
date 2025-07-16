using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.Services;

namespace ClashOpenings.Presentation.ViewModels;

/// <summary>
///     ViewModel para a interface de usuário de detecção de conflitos de aberturas em lajes.
///     Gerencia a seleção de modelos vinculados, o status da operação e a execução do comando de detecção.
/// </summary>
public class SlabsOpeningsViewModel : INotifyPropertyChanged
{
    private ICommand _runClashDetectionCommand;
    private RevitLinkInstance? _selectedLinkInstance1;
    private RevitLinkInstance? _selectedLinkInstance2;
    private string _statusMessage = string.Empty; // Inicializa com valor padrão

    /// <summary>
    ///     Inicializa uma nova instância da classe <see cref="SlabsOpeningsViewModel" />.
    ///     Configura a coleção de instâncias de link do Revit e o comando para detecção de conflitos.
    /// </summary>
    /// <param name="uiDoc">O documento UI do Revit ativo.</param>
    public SlabsOpeningsViewModel(UIDocument uiDoc)
    {
        var document = uiDoc.Document;

        LinkInstances = new ObservableCollection<RevitLinkInstance>(ElementCollector.GetAllLinkInstances(document));

        var eventHandler = new SlabsOpeningsExternalEventHandler();
        eventHandler.SetStatusCallback(status => StatusMessage = status);
        var externalEvent = ExternalEvent.Create(eventHandler);

        _runClashDetectionCommand = new SlabsOpeningsRelayCommand(
            obj =>
            {
                try
                {
                    eventHandler.SetLinks(SelectedLinkInstance1, SelectedLinkInstance2);
                    externalEvent.Raise();
                    StatusMessage = "Executando detecção de conflitos...";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Erro: {ex.Message}";
                    TaskDialog.Show("Erro", $"Falha ao executar a detecção de conflitos: {ex.Message}");
                }
            },
            obj => SelectedLinkInstance1 != null &&
                   SelectedLinkInstance2 != null &&
                   SelectedLinkInstance1.Id != SelectedLinkInstance2.Id);
    }

    /// <summary>
    ///     Obtém ou define a coleção observável de instâncias de modelos vinculados do Revit disponíveis.
    /// </summary>
    public ObservableCollection<RevitLinkInstance> LinkInstances { get; set; }

    /// <summary>
    ///     Obtém ou define a primeira instância de modelo vinculado selecionada para detecção de conflitos.
    /// </summary>
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

    /// <summary>
    ///     Obtém ou define a segunda instância de modelo vinculado selecionada para detecção de conflitos.
    /// </summary>
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

    /// <summary>
    ///     Obtém ou define a mensagem de status atual a ser exibida na interface do usuário.
    /// </summary>
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

    /// <summary>
    ///     Obtém ou define o comando para iniciar a detecção de conflitos.
    /// </summary>
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

    /// <summary>
    ///     Evento acionado quando uma propriedade tem seu valor alterado.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Dispara o evento <see cref="PropertyChanged" />.
    /// </summary>
    /// <param name="propertyName">O nome da propriedade que foi alterada.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}