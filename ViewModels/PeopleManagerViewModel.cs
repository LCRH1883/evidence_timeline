using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using evidence_timeline.Models;

namespace evidence_timeline.ViewModels
{
    public class PeopleManagerViewModel : BaseViewModel
    {
        private PersonEntry? _selectedPerson;

        public PeopleManagerViewModel(IEnumerable<Person> people)
        {
            People = new ObservableCollection<PersonEntry>(people.Select(p => new PersonEntry(p.Id, p.Name, p.Notes, p.Aliases)));

            AddPersonCommand = new RelayCommand(AddPerson);
            DeletePersonCommand = new RelayCommand(DeletePerson, () => SelectedPerson != null);

            People.CollectionChanged += OnPeopleCollectionChanged;
            foreach (var person in People)
            {
                HookPerson(person);
            }
        }

        public ObservableCollection<PersonEntry> People { get; }

        public PersonEntry? SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                if (SetProperty(ref _selectedPerson, value))
                {
                    RaiseCommandStates();
                }
            }
        }

        public bool HasChanges { get; private set; }

        public ICommand AddPersonCommand { get; }
        public ICommand DeletePersonCommand { get; }

        public List<Person> ToPeople()
        {
            return People.Select(p => p.ToPerson()).ToList();
        }

        private void AddPerson()
        {
            var person = new PersonEntry(Guid.NewGuid().ToString("N"), "New Person", string.Empty, Array.Empty<string>());
            People.Add(person);
            SelectedPerson = person;
            HasChanges = true;
        }

        private void DeletePerson()
        {
            if (SelectedPerson == null)
            {
                return;
            }

            People.Remove(SelectedPerson);
            SelectedPerson = People.FirstOrDefault();
            HasChanges = true;
        }

        private void OnPeopleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            HasChanges = true;

            if (e.NewItems != null)
            {
                foreach (var person in e.NewItems.OfType<PersonEntry>())
                {
                    HookPerson(person);
                }
            }
        }

        private void HookPerson(PersonEntry person)
        {
            person.PropertyChanged += (_, _) =>
            {
                HasChanges = true;
            };

            person.Aliases.CollectionChanged += (_, _) => HasChanges = true;
        }

        private void RaiseCommandStates()
        {
            if (DeletePersonCommand is RelayCommand delete) delete.RaiseCanExecuteChanged();
        }
    }
}
