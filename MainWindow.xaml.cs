using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using evidence_timeline.ViewModels;
using evidence_timeline.Views;

namespace evidence_timeline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            if (vm.CurrentCase != null)
            {
                return;
            }

            var start = new StartDialog
            {
                Owner = this
            };

            var result = start.ShowDialog();
            if (result == true)
            {
                if (start.IsCreate)
                {
                    vm.CreateCaseCommand.Execute(null);
                }
                else
                {
                    vm.OpenCaseCommand.Execute(null);
                }
            }
        }

        private void OnEvidenceRowDoubleClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm || vm.SelectedSummary == null || vm.CurrentCase == null)
            {
                return;
            }

            var evidenceId = vm.SelectedSummary.Id;
            vm.TryOpenEvidenceWindow(evidenceId, this);
        }

        private void OnMetadataEnterKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (DataContext is MainViewModel vm)
            {
                vm.RequestMetadataAutoSave(true);
            }

            e.Handled = true;
        }

        private void OnMetadataSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.RequestMetadataAutoSave(true);
            }
        }

        private void OnMetadataFieldLostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.RequestMetadataAutoSave();
            }
        }

        private void OnMetadataCheckboxChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.CheckBox checkBox || checkBox.DataContext is not SelectableItem)
            {
                return;
            }

            if (DataContext is MainViewModel vm)
            {
                vm.RequestMetadataAutoSave();
            }
        }
    }
}
