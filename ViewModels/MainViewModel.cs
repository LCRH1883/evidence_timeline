using System.Collections.ObjectModel;
using System.Windows.Input;
using evidence_timeline.Models;

namespace evidence_timeline.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private CaseInfo? _currentCase;
        private EvidenceSummary? _selectedEvidence;
        private string _searchText = string.Empty;
        private Tag? _selectedTag;
        private EvidenceType? _selectedType;
        private Person? _selectedPerson;
        private bool _isLeftPaneVisible = true;
        private bool _isRightPaneVisible = true;
        private bool _isBottomPaneVisible = true;
        private string _notesText = string.Empty;

        public MainViewModel()
        {
            CreateCaseCommand = new RelayCommand(() => { });
            OpenCaseCommand = new RelayCommand(() => { });
            NewEvidenceCommand = new RelayCommand(() => { });
            OpenEvidenceWindowCommand = new RelayCommand(() => { });
            AddAttachmentCommand = new RelayCommand(() => { });
            SaveNotesCommand = new RelayCommand(() => { });
        }

        public CaseInfo? CurrentCase
        {
            get => _currentCase;
            set => SetProperty(ref _currentCase, value);
        }

        public ObservableCollection<EvidenceSummary> EvidenceList { get; } = new();
        public ObservableCollection<Tag> Tags { get; } = new();
        public ObservableCollection<EvidenceType> EvidenceTypes { get; } = new();
        public ObservableCollection<Person> People { get; } = new();

        public EvidenceSummary? SelectedEvidence
        {
            get => _selectedEvidence;
            set => SetProperty(ref _selectedEvidence, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public Tag? SelectedTag
        {
            get => _selectedTag;
            set => SetProperty(ref _selectedTag, value);
        }

        public EvidenceType? SelectedType
        {
            get => _selectedType;
            set => SetProperty(ref _selectedType, value);
        }

        public Person? SelectedPerson
        {
            get => _selectedPerson;
            set => SetProperty(ref _selectedPerson, value);
        }

        public bool IsLeftPaneVisible
        {
            get => _isLeftPaneVisible;
            set => SetProperty(ref _isLeftPaneVisible, value);
        }

        public bool IsRightPaneVisible
        {
            get => _isRightPaneVisible;
            set => SetProperty(ref _isRightPaneVisible, value);
        }

        public bool IsBottomPaneVisible
        {
            get => _isBottomPaneVisible;
            set => SetProperty(ref _isBottomPaneVisible, value);
        }

        public string NotesText
        {
            get => _notesText;
            set => SetProperty(ref _notesText, value);
        }

        public ICommand CreateCaseCommand { get; }
        public ICommand OpenCaseCommand { get; }
        public ICommand NewEvidenceCommand { get; }
        public ICommand OpenEvidenceWindowCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand SaveNotesCommand { get; }
    }
}
