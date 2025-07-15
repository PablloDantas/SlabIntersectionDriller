using System.Windows;

namespace ClashOpenings.Presentation.Views
{
    public partial class ClashSelectionView : Window
    {
        public ClashSelectionView(object viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            helper.Owner = Autodesk.Windows.ComponentManager.ApplicationWindow;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
