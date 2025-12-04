using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using evidence_timeline.Models;
using evidence_timeline.Utilities;
using MessageBox = System.Windows.MessageBox;

namespace evidence_timeline.ViewModels
{
    public class CaseSettingsViewModel : BaseViewModel
    {
        private bool _showLeftPane;
        private bool _showRightPane;
        private bool _showBottomPane;
        private bool _sortNewestFirst;
        private EvidenceTypeEntry? _selectedType;
        private PersonEntry? _selectedPerson;

        public CaseSettingsViewModel(CaseSettings settings, IEnumerable<EvidenceType> types, IEnumerable<Person> people)
        {
            _showLeftPane = settings.ShowLeftPane;
            _showRightPane = settings.ShowRightPane;
            _showBottomPane = settings.ShowBottomPane;
            _sortNewestFirst = settings.SortNewestFirst;

            EvidenceTypes = new ObservableCollection<EvidenceTypeEntry>(types.Select(t => new EvidenceTypeEntry(t.Id, t.Name, t.Description)));

            People = new ObservableCollection<PersonEntry>(people.Select(p =>
                new PersonEntry(p.Id, p.Name, p.Notes, p.Aliases)));

            AddTypeCommand = new RelayCommand(AddType);
            RenameTypeCommand = new RelayCommand(RenameType, () => SelectedType != null);
            DeleteTypeCommand = new RelayCommand(DeleteType, () => SelectedType != null);

            AddPersonCommand = new RelayCommand(AddPerson);
            DeletePersonCommand = new RelayCommand<PersonEntry?>(DeletePerson, p => p != null);

            EvidenceTypes.CollectionChanged += OnTypesCollectionChanged;
            People.CollectionChanged += OnPeopleCollectionChanged;
            foreach (var person in People)
            {
                HookPerson(person);
            }
            foreach (var type in EvidenceTypes)
            {
                HookType(type);
            }
        }

        public ObservableCollection<EvidenceTypeEntry> EvidenceTypes { get; }
        public ObservableCollection<PersonEntry> People { get; }

        public bool ShowLeftPane
        {
            get => _showLeftPane;
            set => SetProperty(ref _showLeftPane, value);
        }

        public bool ShowRightPane
        {
            get => _showRightPane;
            set => SetProperty(ref _showRightPane, value);
        }

        public bool ShowBottomPane
        {
            get => _showBottomPane;
            set => SetProperty(ref _showBottomPane, value);
        }

        public bool SortNewestFirst
        {
            get => _sortNewestFirst;
            set => SetProperty(ref _sortNewestFirst, value);
        }

        public EvidenceTypeEntry? SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetProperty(ref _selectedType, value))
                {
                    RaiseTypeCommandStates();
                }
            }
        }

        public PersonEntry? SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                if (SetProperty(ref _selectedPerson, value))
                {
                    RaisePersonCommandStates();
                }
            }
        }

        public bool HasTypeChanges { get; private set; }
        public bool HasPeopleChanges { get; internal set; }

        public ICommand AddTypeCommand { get; }
        public ICommand RenameTypeCommand { get; }
        public ICommand DeleteTypeCommand { get; }
        public ICommand AddPersonCommand { get; }
        public ICommand DeletePersonCommand { get; }

        public CaseSettings ToCaseSettings()
        {
            return new CaseSettings
            {
                ShowLeftPane = ShowLeftPane,
                ShowRightPane = ShowRightPane,
                ShowBottomPane = ShowBottomPane,
                SortNewestFirst = SortNewestFirst
            };
        }

        private void AddType()
        {
            var owner = UIHelpers.GetActiveWindow();
            var name = UIHelpers.PromptForText("Add Type", "Enter type name:", string.Empty, owner);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (EvidenceTypes.Any(t => string.Equals(t.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("A type with that name already exists.", "Duplicate Type", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var type = new EvidenceTypeEntry(Guid.NewGuid().ToString("N"), name.Trim());
            EvidenceTypes.Add(type);
            SelectedType = type;
            HasTypeChanges = true;
            RaiseTypeCommandStates();
        }

        private void RenameType()
        {
            if (SelectedType == null)
            {
                return;
            }

            var owner = UIHelpers.GetActiveWindow();
            var newName = UIHelpers.PromptForText("Rename Type", "Enter new type name:", SelectedType.Name, owner);
            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            if (EvidenceTypes.Any(t => !string.Equals(t.Id, SelectedType.Id, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Name, newName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("A type with that name already exists.", "Duplicate Type", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedType.Name = newName.Trim();
            HasTypeChanges = true;
        }

        private void DeleteType()
        {
            if (SelectedType == null)
            {
                return;
            }

            var result = MessageBox.Show($"Delete type '{SelectedType.Name}'? This will clear the type from any evidence using it.", "Delete Type", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var toRemove = SelectedType;
            EvidenceTypes.Remove(toRemove);
            SelectedType = EvidenceTypes.FirstOrDefault();
            HasTypeChanges = true;
            RaiseTypeCommandStates();
        }

        private void AddPerson()
        {
            var owner = UIHelpers.GetActiveWindow();
            var name = UIHelpers.PromptForText("Add Person", "Enter person name:", string.Empty, owner);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (People.Any(p => string.Equals(p.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("A person with that name already exists.", "Duplicate Person", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var person = new PersonEntry(Guid.NewGuid().ToString("N"), name.Trim(), string.Empty, Array.Empty<string>());
            People.Add(person);
            HasPeopleChanges = true;
            RaisePersonCommandStates();
        }

        private void DeletePerson(PersonEntry? person)
        {
            if (person == null)
            {
                return;
            }

            var result = MessageBox.Show($"Delete person '{person.Name}'? This will remove them from any evidence.", "Delete Person", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            People.Remove(person);
            HasPeopleChanges = true;
            RaisePersonCommandStates();
        }

        private void RaiseTypeCommandStates()
        {
            if (AddTypeCommand is RelayCommand add) add.RaiseCanExecuteChanged();
            if (RenameTypeCommand is RelayCommand rename) rename.RaiseCanExecuteChanged();
            if (DeleteTypeCommand is RelayCommand delete) delete.RaiseCanExecuteChanged();
        }

        private void RaisePersonCommandStates()
        {
            if (AddPersonCommand is RelayCommand add) add.RaiseCanExecuteChanged();
            if (DeletePersonCommand is RelayCommand<PersonEntry?> delete) delete.RaiseCanExecuteChanged();
        }

        private void HookType(EvidenceTypeEntry type)
        {
            type.PropertyChanged += OnTypePropertyChanged;
        }

        private void UnhookType(EvidenceTypeEntry type)
        {
            type.PropertyChanged -= OnTypePropertyChanged;
        }

        private void OnTypePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HasTypeChanges = true;
        }

        private void HookPerson(PersonEntry person)
        {
            person.PropertyChanged += (_, _) => HasPeopleChanges = true;
            person.Aliases.CollectionChanged += (_, _) => HasPeopleChanges = true;
        }

        private void OnTypesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            HasTypeChanges = true;

            if (e.NewItems != null)
            {
                foreach (var type in e.NewItems.OfType<EvidenceTypeEntry>())
                {
                    HookType(type);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var type in e.OldItems.OfType<EvidenceTypeEntry>())
                {
                    UnhookType(type);
                }
            }
        }

        private void OnPeopleCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HasPeopleChanges = true;

            if (e.NewItems != null)
            {
                foreach (var person in e.NewItems.OfType<PersonEntry>())
                {
                    HookPerson(person);
                }
            }
        }
    }
}
