using System;
using System.Linq;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace evidence_timeline.Views
{
    public partial class CaseSettingsWindow : Window
    {
        public CaseSettingsWindow()
        {
            InitializeComponent();
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnAddPersonClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ViewModels.CaseSettingsViewModel vm)
            {
                return;
            }

            var entry = new ViewModels.PersonEntry(Guid.NewGuid().ToString("N"), string.Empty, string.Empty, Array.Empty<string>());
            var dialog = new PersonDialog
            {
                Owner = Owner,
                DataContext = entry
            };

            if (dialog.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    MessageBox.Show("Name is required.", "Invalid Person", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (vm.People.Any(p => string.Equals(p.Name, entry.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A person with that name already exists.", "Duplicate Person", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                vm.People.Add(entry);
                vm.SelectedPerson = entry;
            }
        }

        private void OnEditPersonClicked(object sender, RoutedEventArgs e)
        {
            EditSelectedPerson();
        }

        private void OnPersonDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditSelectedPerson();
        }

        private void EditSelectedPerson()
        {
            if (DataContext is not ViewModels.CaseSettingsViewModel vm || vm.SelectedPerson == null)
            {
                return;
            }

            var original = vm.SelectedPerson;
            var working = original.Clone();

            var dialog = new PersonDialog
            {
                Owner = Owner,
                DataContext = working
            };

            if (dialog.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(working.Name))
                {
                    MessageBox.Show("Name is required.", "Invalid Person", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (vm.People.Any(p => !string.Equals(p.Id, working.Id, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.Name, working.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A person with that name already exists.", "Duplicate Person", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                original.ApplyFrom(working);
                vm.HasPeopleChanges = true;
            }
        }
    }
}
