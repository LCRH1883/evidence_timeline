using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using evidence_timeline.Models;
using evidence_timeline.Utilities;
using MessageBox = System.Windows.MessageBox;

namespace evidence_timeline.ViewModels
{
    public class PersonEntry : BaseViewModel
    {
        private string _name = string.Empty;
        private string _notes = string.Empty;

        public PersonEntry(string id, string name, string notes, IEnumerable<string> aliases)
        {
            Id = id;
            _name = name;
            _notes = notes;
            Aliases = new ObservableCollection<string>(aliases ?? Array.Empty<string>());

            AddAliasCommand = new RelayCommand(AddAlias);
            RemoveAliasCommand = new RelayCommand<string>(RemoveAlias);
        }

        public string Id { get; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public ObservableCollection<string> Aliases { get; }

        public ICommand AddAliasCommand { get; }
        public ICommand RemoveAliasCommand { get; }

        public Person ToPerson()
        {
            return new Person
            {
                Id = Id,
                Name = Name?.Trim() ?? string.Empty,
                Notes = Notes ?? string.Empty,
                Aliases = Aliases.Select(a => a ?? string.Empty).Where(a => !string.IsNullOrWhiteSpace(a)).ToList()
            };
        }

        public PersonEntry Clone()
        {
            return new PersonEntry(Id, Name, Notes, Aliases.ToList());
        }

        public void ApplyFrom(PersonEntry other)
        {
            Name = other.Name;
            Notes = other.Notes;

            Aliases.Clear();
            foreach (var alias in other.Aliases)
            {
                Aliases.Add(alias);
            }
        }

        private void AddAlias()
        {
            var alias = UIHelpers.PromptForText("Add Alias", "Enter alias:");
            if (string.IsNullOrWhiteSpace(alias))
            {
                return;
            }

            if (Aliases.Any(a => string.Equals(a, alias.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Alias already exists for this person.", "Duplicate Alias", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Aliases.Add(alias.Trim());
        }

        private void RemoveAlias(string? alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                return;
            }

            Aliases.Remove(alias);
        }
    }
}
