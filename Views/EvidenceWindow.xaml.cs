using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using evidence_timeline.ViewModels;

namespace evidence_timeline.Views
{
    public partial class EvidenceWindow : Window
    {
        public EvidenceWindow()
        {
            InitializeComponent();
        }

        private void OnMetadataEnterKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (DataContext is EvidenceWindowViewModel vm)
            {
                vm.RequestMetadataAutoSave(true);
            }

            e.Handled = true;
        }

        private void OnMetadataSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is EvidenceWindowViewModel vm)
            {
                vm.RequestMetadataAutoSave(true);
            }
        }

        private void OnMetadataFieldLostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is EvidenceWindowViewModel vm)
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

            if (DataContext is EvidenceWindowViewModel vm)
            {
                vm.RequestMetadataAutoSave();
            }
        }
    }
}
