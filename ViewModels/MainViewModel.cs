using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using evidence_timeline.Models;
using evidence_timeline.Services;
using evidence_timeline.Utilities;
using MessageBox = System.Windows.MessageBox;

namespace evidence_timeline.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ICaseStorageService _caseStorage = new CaseStorageService();
        private readonly IReferenceDataService _referenceData = new ReferenceDataService();
        private readonly IEvidenceStorageService _evidenceStorage = new EvidenceStorageService();
        private readonly List<EvidenceSummary> _allEvidenceSummaries = new();
        private readonly Dictionary<string, Evidence> _evidenceById = new();
        private readonly Dictionary<string, Tag> _tagLookup = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EvidenceType> _typeLookup = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Person> _peopleLookup = new(StringComparer.OrdinalIgnoreCase);

        private CaseInfo? _currentCase;
        private EvidenceSummary? _selectedSummary;
        private Evidence? _selectedEvidenceDetail;
        private string _searchText = string.Empty;
        private Tag? _selectedTag;
        private EvidenceType? _selectedType;
        private Person? _selectedPerson;
        private bool _isLeftPaneVisible = true;
        private bool _isRightPaneVisible = true;
        private bool _isBottomPaneVisible = true;
        private string _notesText = string.Empty;
        private bool _isBusy;
        private string _linkedEvidenceText = string.Empty;

        public MainViewModel()
        {
            CreateCaseCommand = new AsyncRelayCommand(CreateCaseAsync);
            OpenCaseCommand = new AsyncRelayCommand(OpenCaseAsync);
            SaveCaseCommand = new AsyncRelayCommand(SaveCaseAsync, () => CurrentCase != null);
            NewEvidenceCommand = new AsyncRelayCommand(NewEvidenceAsync, () => CurrentCase != null);
            OpenEvidenceWindowCommand = new RelayCommand(() => { }, () => SelectedSummary != null);
            AddAttachmentCommand = new RelayCommand(() => { }, () => SelectedSummary != null);
            SaveNotesCommand = new AsyncRelayCommand(SaveNotesAsync, () => SelectedEvidenceDetail != null && CurrentCase != null);
            SaveMetadataCommand = new AsyncRelayCommand(SaveMetadataAsync, () => SelectedEvidenceDetail != null && CurrentCase != null);
        }

        public CaseInfo? CurrentCase
        {
            get => _currentCase;
            set
            {
                if (SetProperty(ref _currentCase, value))
                {
                    RaiseCommandStates();
                }
            }
        }

        public ObservableCollection<EvidenceSummary> EvidenceList { get; } = new();
        public ObservableCollection<Tag> Tags { get; } = new();
        public ObservableCollection<EvidenceType> EvidenceTypes { get; } = new();
        public ObservableCollection<Person> People { get; } = new();
        public ObservableCollection<SelectableItem> TagOptions { get; } = new();
        public ObservableCollection<SelectableItem> PersonOptions { get; } = new();

        public EvidenceSummary? SelectedSummary
        {
            get => _selectedSummary;
            set
            {
                if (SetProperty(ref _selectedSummary, value))
                {
                    _ = LoadSelectedEvidenceAsync(value);
                    RaiseCommandStates();
                }
            }
        }

        public Evidence? SelectedEvidenceDetail
        {
            get => _selectedEvidenceDetail;
            private set
            {
                if (SetProperty(ref _selectedEvidenceDetail, value))
                {
                    OnPropertyChanged(nameof(SelectedEvidenceTagNames));
                    OnPropertyChanged(nameof(SelectedEvidencePersonNames));
                    OnPropertyChanged(nameof(SelectedDateMode));
                    OnPropertyChanged(nameof(ExactDate));
                    OnPropertyChanged(nameof(StartDate));
                    OnPropertyChanged(nameof(EndDate));
                    OnPropertyChanged(nameof(AroundAmount));
                    OnPropertyChanged(nameof(AroundUnit));
                    LinkedEvidenceText = value == null ? string.Empty : string.Join(", ", value.LinkedEvidenceIds ?? Enumerable.Empty<string>());
                    RaiseCommandStates();
                }
            }
        }

        public IEnumerable<string> SelectedEvidenceTagNames =>
            SelectedEvidenceDetail == null
                ? Array.Empty<string>()
                : SelectedEvidenceDetail.TagIds.Select(id => _tagLookup.TryGetValue(id, out var tag) ? tag.Name : id);

        public IEnumerable<string> SelectedEvidencePersonNames =>
            SelectedEvidenceDetail == null
                ? Array.Empty<string>()
                : SelectedEvidenceDetail.PersonIds.Select(id => _peopleLookup.TryGetValue(id, out var person) ? person.Name : id);

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        public Tag? SelectedTag
        {
            get => _selectedTag;
            set
            {
                if (SetProperty(ref _selectedTag, value))
                {
                    ApplyFilters();
                }
            }
        }

        public EvidenceType? SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetProperty(ref _selectedType, value))
                {
                    ApplyFilters();
                }
            }
        }

        public Person? SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                if (SetProperty(ref _selectedPerson, value))
                {
                    ApplyFilters();
                }
            }
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

        public bool IsBusy
        {
            get => _isBusy;
            private set => SetProperty(ref _isBusy, value);
        }

        public EvidenceDateMode SelectedDateMode
        {
            get => SelectedEvidenceDetail?.DateInfo.Mode ?? EvidenceDateMode.Exact;
            set
            {
                if (SelectedEvidenceDetail == null)
                {
                    return;
                }

                if (SelectedEvidenceDetail.DateInfo.Mode != value)
                {
                    SelectedEvidenceDetail.DateInfo.Mode = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? ExactDate
        {
            get => SelectedEvidenceDetail?.DateInfo.ExactDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                if (SelectedEvidenceDetail == null)
                {
                    return;
                }

                SelectedEvidenceDetail.DateInfo.ExactDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
                OnPropertyChanged();
            }
        }

        public DateTime? StartDate
        {
            get => SelectedEvidenceDetail?.DateInfo.StartDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                if (SelectedEvidenceDetail == null)
                {
                    return;
                }

                SelectedEvidenceDetail.DateInfo.StartDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
                OnPropertyChanged();
            }
        }

        public DateTime? EndDate
        {
            get => SelectedEvidenceDetail?.DateInfo.EndDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                if (SelectedEvidenceDetail == null)
                {
                    return;
                }

                SelectedEvidenceDetail.DateInfo.EndDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
                OnPropertyChanged();
            }
        }

        public int? AroundAmount
        {
            get => SelectedEvidenceDetail?.DateInfo.AroundAmount;
            set
            {
                if (SelectedEvidenceDetail == null)
                {
                    return;
                }

                SelectedEvidenceDetail.DateInfo.AroundAmount = value;
                OnPropertyChanged();
            }
        }

        public string? AroundUnit
        {
            get => SelectedEvidenceDetail?.DateInfo.AroundUnit;
            set
            {
                if (SelectedEvidenceDetail == null)
                {
                    return;
                }

                SelectedEvidenceDetail.DateInfo.AroundUnit = value;
                OnPropertyChanged();
            }
        }

        public string LinkedEvidenceText
        {
            get => _linkedEvidenceText;
            set => SetProperty(ref _linkedEvidenceText, value);
        }

        public ICommand CreateCaseCommand { get; }
        public ICommand OpenCaseCommand { get; }
        public ICommand SaveCaseCommand { get; }
        public ICommand NewEvidenceCommand { get; }
        public ICommand OpenEvidenceWindowCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand SaveNotesCommand { get; }
        public ICommand SaveMetadataCommand { get; }

        private async Task CreateCaseAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                var root = UIHelpers.PickFolder("Select a folder to create the case in");
                if (string.IsNullOrWhiteSpace(root))
                {
                    return;
                }

                var name = UIHelpers.PromptForText("New Case", "Enter case name:");
                if (string.IsNullOrWhiteSpace(name))
                {
                    return;
                }

                var number = UIHelpers.PromptForText("New Case", "Enter case number (optional):") ?? string.Empty;

                var caseInfo = await _caseStorage.CreateCaseAsync(root, name, number);
                await LoadCaseDataAsync(caseInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to create case: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OpenCaseAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                var folder = UIHelpers.PickFolder("Select case folder (contains case.json)");
                if (string.IsNullOrWhiteSpace(folder))
                {
                    return;
                }

                var caseInfo = await _caseStorage.LoadCaseAsync(folder);
                await LoadCaseDataAsync(caseInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open case: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadCaseDataAsync(CaseInfo caseInfo)
        {
            CurrentCase = caseInfo;

            Tags.Clear();
            EvidenceTypes.Clear();
            People.Clear();
            _tagLookup.Clear();
            _typeLookup.Clear();
            _peopleLookup.Clear();
            _allEvidenceSummaries.Clear();
            _evidenceById.Clear();
            TagOptions.Clear();
            PersonOptions.Clear();

            var tags = await _referenceData.LoadTagsAsync(caseInfo);
            foreach (var tag in tags)
            {
                Tags.Add(tag);
                _tagLookup[tag.Id] = tag;
                TagOptions.Add(new SelectableItem(tag.Id, tag.Name));
            }

            var types = await _referenceData.LoadTypesAsync(caseInfo);
            foreach (var type in types)
            {
                EvidenceTypes.Add(type);
                _typeLookup[type.Id] = type;
            }

            var people = await _referenceData.LoadPeopleAsync(caseInfo);
            foreach (var person in people)
            {
                People.Add(person);
                _peopleLookup[person.Id] = person;
                PersonOptions.Add(new SelectableItem(person.Id, person.Name));
            }

            var evidence = await _evidenceStorage.LoadAllEvidenceAsync(caseInfo);
            foreach (var item in evidence)
            {
                _evidenceById[item.Id] = item;
                _allEvidenceSummaries.Add(BuildSummary(item));
            }

            ApplyFilters();
            SelectedSummary = EvidenceList.FirstOrDefault();
            NotesText = string.Empty;
            SyncSelectionOptions(null);
        }

        private EvidenceSummary BuildSummary(Evidence evidence)
        {
            var typeName = _typeLookup.TryGetValue(evidence.TypeId, out var type) ? type.Name : string.Empty;
            var tagNames = evidence.TagIds.Select(id => _tagLookup.TryGetValue(id, out var tag) ? tag.Name : id).ToArray();
            var personNames = evidence.PersonIds.Select(id => _peopleLookup.TryGetValue(id, out var person) ? person.Name : id).ToArray();

            return new EvidenceSummary
            {
                Id = evidence.Id,
                EvidenceNumber = evidence.EvidenceNumber,
                Title = evidence.Title,
                CourtNumber = evidence.CourtNumber,
                TypeName = typeName,
                TagNames = tagNames,
                PersonNames = personNames,
                DateDisplay = ResolveDateDisplay(evidence.DateInfo),
                SortDate = evidence.DateInfo.SortDate.ToDateTime(TimeOnly.MinValue),
                SearchKey = BuildSearchKey(evidence.Title, evidence.CourtNumber, typeName, tagNames, personNames)
            };
        }

        private static string BuildSearchKey(string title, string courtNumber, string typeName, IEnumerable<string> tagNames, IEnumerable<string> personNames)
        {
            var parts = new List<string>
            {
                title,
                courtNumber,
                typeName
            };
            parts.AddRange(tagNames);
            parts.AddRange(personNames);
            return string.Join(" ", parts).ToLowerInvariant();
        }

        private static string ResolveDateDisplay(EvidenceDateInfo dateInfo)
        {
            if (dateInfo.Mode == EvidenceDateMode.Around && dateInfo.AroundAmount.HasValue && !string.IsNullOrWhiteSpace(dateInfo.AroundUnit) && dateInfo.ExactDate.HasValue)
            {
                return $"~{dateInfo.AroundAmount} {dateInfo.AroundUnit} of {dateInfo.ExactDate.Value:yyyy-MM-dd}";
            }

            if (dateInfo.Mode == EvidenceDateMode.Broad && dateInfo.StartDate.HasValue && dateInfo.EndDate.HasValue)
            {
                return $"{dateInfo.StartDate.Value:yyyy-MM-dd} - {dateInfo.EndDate.Value:yyyy-MM-dd}";
            }

            if (dateInfo.ExactDate.HasValue)
            {
                return dateInfo.ExactDate.Value.ToString("yyyy-MM-dd");
            }

            if (dateInfo.StartDate.HasValue)
            {
                return dateInfo.StartDate.Value.ToString("yyyy-MM-dd");
            }

            if (dateInfo.EndDate.HasValue)
            {
                return dateInfo.EndDate.Value.ToString("yyyy-MM-dd");
            }

            return dateInfo.SortDate != default ? dateInfo.SortDate.ToString("yyyy-MM-dd") : "Unknown";
        }

        private void ApplyFilters()
        {
            var filtered = _allEvidenceSummaries.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var needle = SearchText.ToLowerInvariant();
                filtered = filtered.Where(s => s.SearchKey.Contains(needle, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedTag != null)
            {
                filtered = filtered.Where(s => s.TagNames.Any(t => string.Equals(t, SelectedTag.Name, StringComparison.OrdinalIgnoreCase)));
            }

            if (SelectedType != null)
            {
                filtered = filtered.Where(s => string.Equals(s.TypeName, SelectedType.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedPerson != null)
            {
                filtered = filtered.Where(s => s.PersonNames.Any(p => string.Equals(p, SelectedPerson.Name, StringComparison.OrdinalIgnoreCase)));
            }

            filtered = filtered.OrderByDescending(s => s.SortDate).ThenBy(s => s.EvidenceNumber);

            EvidenceList.Clear();
            foreach (var item in filtered)
            {
                EvidenceList.Add(item);
            }
        }

        private async Task NewEvidenceAsync()
        {
            if (CurrentCase == null)
            {
                return;
            }

            var title = UIHelpers.PromptForText("New Evidence", "Enter evidence title:");
            if (string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            var now = DateTime.UtcNow;
            var evidence = new Evidence
            {
                Title = title,
                EvidenceNumber = 0,
                CourtNumber = string.Empty,
                TypeId = EvidenceTypes.FirstOrDefault()?.Id ?? string.Empty,
                DateInfo = new EvidenceDateInfo
                {
                    Mode = EvidenceDateMode.Exact,
                    ExactDate = DateOnly.FromDateTime(now),
                    SortDate = DateOnly.FromDateTime(now)
                }
            };

            evidence = await _evidenceStorage.CreateEvidenceAsync(CurrentCase, evidence);
            _evidenceById[evidence.Id] = evidence;
            var summary = BuildSummary(evidence);
            _allEvidenceSummaries.Add(summary);
            ApplyFilters();
            SelectedSummary = EvidenceList.FirstOrDefault(s => s.Id == summary.Id);
        }

        private async Task LoadSelectedEvidenceAsync(EvidenceSummary? summary)
        {
            if (summary == null || CurrentCase == null)
            {
                SelectedEvidenceDetail = null;
                NotesText = string.Empty;
                return;
            }

            if (_evidenceById.TryGetValue(summary.Id, out var evidence))
            {
                SelectedEvidenceDetail = evidence;
                SyncSelectionOptions(evidence);
                await LoadNotesAsync(evidence);
            }
            else
            {
                SelectedEvidenceDetail = null;
                NotesText = string.Empty;
                SyncSelectionOptions(null);
            }
        }

        private async Task LoadNotesAsync(Evidence evidence)
        {
            if (CurrentCase == null)
            {
                NotesText = string.Empty;
                return;
            }

            var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(CurrentCase, evidence);
            var noteFile = evidence.NoteFile ?? "note.md";
            var notePath = Path.Combine(folder, noteFile);

            if (File.Exists(notePath))
            {
                NotesText = await File.ReadAllTextAsync(notePath);
            }
            else
            {
                NotesText = string.Empty;
            }
        }

        private async Task SaveNotesAsync()
        {
            if (CurrentCase == null || SelectedEvidenceDetail == null)
            {
                return;
            }

            try
            {
                var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(CurrentCase, SelectedEvidenceDetail);
                var noteFile = SelectedEvidenceDetail.NoteFile ?? "note.md";
                var notePath = Path.Combine(folder, noteFile);
                Directory.CreateDirectory(folder);
                await File.WriteAllTextAsync(notePath, NotesText ?? string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save notes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveCaseAsync()
        {
            if (CurrentCase == null)
            {
                return;
            }

            try
            {
                await _caseStorage.SaveCaseAsync(CurrentCase);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save case: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SyncSelectionOptions(Evidence? evidence)
        {
            foreach (var option in TagOptions)
            {
                option.IsSelected = evidence != null && evidence.TagIds.Any(id => string.Equals(id, option.Id, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var option in PersonOptions)
            {
                option.IsSelected = evidence != null && evidence.PersonIds.Any(id => string.Equals(id, option.Id, StringComparison.OrdinalIgnoreCase));
            }
        }

        private async Task SaveMetadataAsync()
        {
            if (CurrentCase == null || SelectedEvidenceDetail == null)
            {
                return;
            }

            try
            {
                SelectedEvidenceDetail.TagIds = TagOptions.Where(o => o.IsSelected).Select(o => o.Id).ToList();
                SelectedEvidenceDetail.PersonIds = PersonOptions.Where(o => o.IsSelected).Select(o => o.Id).ToList();
                SelectedEvidenceDetail.LinkedEvidenceIds = ParseLinkedEvidenceIds(LinkedEvidenceText);

                UpdateSortDate(SelectedEvidenceDetail);

                await _evidenceStorage.SaveEvidenceAsync(CurrentCase, SelectedEvidenceDetail);

                var updatedSummary = BuildSummary(SelectedEvidenceDetail);
                UpdateSummaryLists(updatedSummary);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save metadata: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSummaryLists(EvidenceSummary updated)
        {
            var idx = _allEvidenceSummaries.FindIndex(s => string.Equals(s.Id, updated.Id, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                _allEvidenceSummaries[idx] = updated;
            }
            else
            {
                _allEvidenceSummaries.Add(updated);
            }

            ApplyFilters();
            SelectedSummary = EvidenceList.FirstOrDefault(s => string.Equals(s.Id, updated.Id, StringComparison.OrdinalIgnoreCase));
        }

        private static List<string> ParseLinkedEvidenceIds(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new List<string>();
            }

            return input
                .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void UpdateSortDate(Evidence evidence)
        {
            if (evidence.DateInfo == null)
            {
                evidence.DateInfo = new EvidenceDateInfo();
            }

            evidence.DateInfo.SortDate = ResolveSortDate(evidence.DateInfo);
        }

        private static DateOnly ResolveSortDate(EvidenceDateInfo dateInfo)
        {
            if (dateInfo.SortDate != default)
            {
                return dateInfo.SortDate;
            }

            return dateInfo.ExactDate
                ?? dateInfo.StartDate
                ?? dateInfo.EndDate
                ?? DateOnly.FromDateTime(DateTime.UtcNow);
        }

        private void RaiseCommandStates()
        {
            if (NewEvidenceCommand is AsyncRelayCommand asyncNewEvidence)
            {
                asyncNewEvidence.RaiseCanExecuteChanged();
            }

            if (OpenEvidenceWindowCommand is RelayCommand relayOpenEvidence)
            {
                relayOpenEvidence.RaiseCanExecuteChanged();
            }

            if (AddAttachmentCommand is RelayCommand relayAddAttachment)
            {
                relayAddAttachment.RaiseCanExecuteChanged();
            }

            if (SaveNotesCommand is AsyncRelayCommand asyncSaveNotes)
            {
                asyncSaveNotes.RaiseCanExecuteChanged();
            }

            if (SaveMetadataCommand is AsyncRelayCommand asyncSaveMetadata)
            {
                asyncSaveMetadata.RaiseCanExecuteChanged();
            }

            if (SaveCaseCommand is AsyncRelayCommand asyncSaveCase)
            {
                asyncSaveCase.RaiseCanExecuteChanged();
            }
        }
    }
}
