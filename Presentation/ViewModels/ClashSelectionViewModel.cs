using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ClashOpenings.Presentation.ViewModels
{
    public class ClashSelectionViewModel : INotifyPropertyChanged
    {
        private RevitLinkInstance _selectedLinkInstance1;
        private RevitLinkInstance _selectedLinkInstance2;

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

        public ICommand RunClashDetectionCommand { get; }

        public ClashSelectionViewModel(UIDocument uiDoc)
        {
            var doc = uiDoc.Document;
            
            var linkInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .ToList();
            LinkInstances = new ObservableCollection<RevitLinkInstance>(linkInstances);

            RunClashDetectionCommand = new RelayCommand(ExecuteClashDetection, CanExecuteClashDetection);
        }

        private bool CanExecuteClashDetection(object obj)
        {
            return SelectedLinkInstance1 != null &&
                   SelectedLinkInstance2 != null &&
                   SelectedLinkInstance1.Id != SelectedLinkInstance2.Id;
        }

        private void ExecuteClashDetection(object obj)
        {
            // The logic is handled in the command, this command is just to enable/disable the button
            // and close the dialog. The main command will retrieve the selected items from the ViewModel.
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

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
}
