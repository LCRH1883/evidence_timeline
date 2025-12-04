using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using evidence_timeline.Models;
using evidence_timeline.Services;
using evidence_timeline.Utilities;
using evidence_timeline.Views;
using MessageBox = System.Windows.MessageBox;
using WinForms = System.Windows.Forms;
using WpfApp = System.Windows.Application;

namespace evidence_timeline.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ICaseStorageService _caseStorage = new CaseStorageService();
        private readonly IReferenceDataService _referenceData = new ReferenceDataService();
        private readonly IEvidenceStorageService _evidenceStorage = new EvidenceStorageService();
        private readonly List<EvidenceSummary> _allEvidenceSummaries = new();
        private readonly Dictionary<string, Evidence> _evidenceById = new();
        private readonly Dictionary<string, EvidenceType> _typeLookup = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Person> _peopleLookup = new(StringComparer.OrdinalIgnoreCase);
        private Evidence? _loadedEvidenceSnapshot;
        private string _loadedNotesSnapshot = string.Empty;
        private CancellationTokenSource? _notesAutoSaveCts;
        private CancellationTokenSource? _metadataAutoSaveCts;
        private bool _suppressNotesAutoSave;
        private bool _suppressMetadataAutoSave;
        private readonly TimeSpan _notesAutoSaveDelay = TimeSpan.FromMilliseconds(800);
        private readonly TimeSpan _metadataAutoSaveDelay = TimeSpan.FromMilliseconds(300);

        private CaseInfo? _currentCase;
        private EvidenceSummary? _selectedSummary;
        private Evidence? _selectedEvidenceDetail;
        private string _searchText = string.Empty;
        private EvidenceType? _selectedType;
        private Person? _selectedPerson;
        private bool _isLeftPaneVisible = true;
        private bool _isRightPaneVisible = true;
        private bool _isBottomPaneVisible = true;
        private string _notesText = string.Empty;
        private double _zoomLevel = 1.0;
        private double _editorFontSize = 9.0;
        private bool _isBusy;
        private string _linkedEvidenceText = string.Empty;
        private bool _sortNewestFirst = true;
        private bool _isLoadingCaseSettings;
        private AppSettings _appSettings = new();
        private CaseSettings _caseSettings = new();

        public MainViewModel()
        {
            CreateCaseCommand = new AsyncRelayCommand(CreateCaseAsync);
            OpenCaseCommand = new AsyncRelayCommand(OpenCaseAsync);
            SaveCaseCommand = new AsyncRelayCommand(SaveCaseAsync, () => CurrentCase != null);
            NewEvidenceCommand = new AsyncRelayCommand(NewEvidenceAsync, () => CurrentCase != null);
            OpenEvidenceWindowCommand = new RelayCommand(OpenEvidenceWindow, () => SelectedSummary != null);
            OpenRecentCaseCommand = new RelayCommand<string>(path => _ = OpenCaseFromPathAsync(path));
            DeleteEvidenceCommand = new AsyncRelayCommand(DeleteEvidenceAsync, () => SelectedSummary != null && CurrentCase != null);
            RemoveRecentCaseCommand = new RelayCommand<string>(RemoveRecentCase);
            AddAttachmentCommand = new AsyncRelayCommand(AddAttachmentAsync, () => SelectedEvidenceDetail != null && CurrentCase != null);
            OpenAttachmentCommand = new RelayCommand<AttachmentInfo>(attachment => _ = OpenAttachmentAsync(attachment));
            OpenAttachmentFolderCommand = new RelayCommand<AttachmentInfo>(attachment => _ = OpenAttachmentFolderAsync(attachment));
            RemoveAttachmentCommand = new RelayCommand<AttachmentInfo>(attachment => _ = RemoveAttachmentAsync(attachment));
            SaveNotesCommand = new AsyncRelayCommand(SaveNotesAsync, () => SelectedEvidenceDetail != null && CurrentCase != null);
            SaveMetadataCommand = new AsyncRelayCommand(SaveMetadataAsync, () => SelectedEvidenceDetail != null && CurrentCase != null);
            ManageLinksCommand = new RelayCommand(ManageLinks, () => SelectedEvidenceDetail != null);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            AddTypeCommand = new AsyncRelayCommand(AddTypeAsync, () => CurrentCase != null);
            RenameTypeCommand = new AsyncRelayCommand(RenameTypeAsync, () => CurrentCase != null && SelectedType != null);
            DeleteTypeCommand = new AsyncRelayCommand(DeleteTypeAsync, () => CurrentCase != null && SelectedType != null);
            AddPersonCommand = new AsyncRelayCommand(AddPersonAsync, () => CurrentCase != null);
            RenamePersonCommand = new AsyncRelayCommand(RenamePersonAsync, () => CurrentCase != null && SelectedPerson != null);
            DeletePersonCommand = new AsyncRelayCommand(DeletePersonAsync, () => CurrentCase != null && SelectedPerson != null);
            OpenPreferencesCommand = new RelayCommand(OpenPreferences);
            OpenCaseSettingsCommand = new AsyncRelayCommand(OpenCaseSettingsAsync, () => CurrentCase != null);
            SetZoomCommand = new RelayCommand<object>(SetZoom);
            SetEditorFontSizeCommand = new RelayCommand<object>(SetEditorFontSize);

            PersonOptions.CollectionChanged += OnPersonOptionsChanged;

            _ = LoadAppSettingsAsync();
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
        public ObservableCollection<EvidenceType> EvidenceTypes { get; } = new();
        public ObservableCollection<Person> People { get; } = new();
        public ObservableCollection<SelectableItem> PersonOptions { get; } = new();
        public bool SortNewestFirst
        {
            get => _sortNewestFirst;
            set
            {
                if (SetProperty(ref _sortNewestFirst, value) && !_isLoadingCaseSettings)
                {
                    ApplyFilters();
                    _ = SaveCaseSettingsAsync();
                }
            }
        }

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
                    _suppressMetadataAutoSave = true;
                    try
                    {
                        OnPropertyChanged(nameof(SelectedEvidencePersonNames));
                        OnPropertyChanged(nameof(SelectedDateMode));
                        OnPropertyChanged(nameof(ExactDate));
                        OnPropertyChanged(nameof(StartDate));
                        OnPropertyChanged(nameof(EndDate));
                        OnPropertyChanged(nameof(AroundAmount));
                        OnPropertyChanged(nameof(AroundUnit));
                        LinkedEvidenceText = value == null ? string.Empty : string.Join(", ", value.LinkedEvidenceIds ?? Enumerable.Empty<string>());
                    }
                    finally
                    {
                        _suppressMetadataAutoSave = false;
                    }

                    RaiseCommandStates();
                }
            }
        }

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
            set
            {
                if (SetProperty(ref _isLeftPaneVisible, value) && !_isLoadingCaseSettings)
                {
                    _ = SaveCaseSettingsAsync();
                }
            }
        }

        public bool IsRightPaneVisible
        {
            get => _isRightPaneVisible;
            set
            {
                if (SetProperty(ref _isRightPaneVisible, value) && !_isLoadingCaseSettings)
                {
                    _ = SaveCaseSettingsAsync();
                }
            }
        }

        public bool IsBottomPaneVisible
        {
            get => _isBottomPaneVisible;
            set
            {
                if (SetProperty(ref _isBottomPaneVisible, value) && !_isLoadingCaseSettings)
                {
                    _ = SaveCaseSettingsAsync();
                }
            }
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (SetProperty(ref _zoomLevel, value) && !_isLoadingCaseSettings)
                {
                    _ = SaveCaseSettingsAsync();
                }
            }
        }

        public double EditorFontSize
        {
            get => _editorFontSize;
            set
            {
                if (SetProperty(ref _editorFontSize, value) && !_isLoadingCaseSettings)
                {
                    if (System.Windows.Application.Current != null)
                    {
                        System.Windows.Application.Current.Resources["EditorFontSize"] = value;
                    }
                    _ = SaveCaseSettingsAsync();
                }
            }
        }

        public string NotesText
        {
            get => _notesText;
            set
            {
                if (SetProperty(ref _notesText, value) && !_suppressNotesAutoSave)
                {
                    QueueNotesAutoSave();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set => SetProperty(ref _isBusy, value);
        }

        public IReadOnlyList<string> RecentCasePaths => (_appSettings?.RecentCases ?? new List<string>()).ToArray();

        private void RemoveRecentCase(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            _appSettings ??= new AppSettings();
            _appSettings.RecentCases.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            _ = SaveAppSettingsAsync();

            OnPropertyChanged(nameof(RecentCasePaths));
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
                    RequestMetadataAutoSave();
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
                RequestMetadataAutoSave();
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
                RequestMetadataAutoSave();
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
                RequestMetadataAutoSave();
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
                RequestMetadataAutoSave();
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
                RequestMetadataAutoSave();
            }
        }

        public string LinkedEvidenceText
        {
            get => _linkedEvidenceText;
            set
            {
                if (SetProperty(ref _linkedEvidenceText, value) && !_suppressMetadataAutoSave)
                {
                    RequestMetadataAutoSave();
                }
            }
        }

        public ICommand CreateCaseCommand { get; }
        public ICommand OpenCaseCommand { get; }
        public ICommand SaveCaseCommand { get; }
        public ICommand NewEvidenceCommand { get; }
        public ICommand OpenEvidenceWindowCommand { get; }
        public ICommand OpenRecentCaseCommand { get; }
        public ICommand DeleteEvidenceCommand { get; }
        public ICommand RemoveRecentCaseCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand OpenAttachmentCommand { get; }
        public ICommand OpenAttachmentFolderCommand { get; }
        public ICommand RemoveAttachmentCommand { get; }
        public ICommand SaveNotesCommand { get; }
        public ICommand SaveMetadataCommand { get; }
        public ICommand ManageLinksCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand AddTypeCommand { get; }
        public ICommand RenameTypeCommand { get; }
        public ICommand DeleteTypeCommand { get; }
        public ICommand AddPersonCommand { get; }
        public ICommand RenamePersonCommand { get; }
        public ICommand DeletePersonCommand { get; }
        public ICommand OpenPreferencesCommand { get; }
        public ICommand OpenCaseSettingsCommand { get; }
        public ICommand SetZoomCommand { get; }
        public ICommand SetEditorFontSizeCommand { get; }

        private async Task CreateCaseAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                var (root, name, number) = PromptForCaseDetails();
                if (root == null || name == null)
                {
                    return;
                }

                var caseInfo = await _caseStorage.CreateCaseAsync(root, name, number ?? string.Empty);
                await LoadCaseDataAsync(caseInfo);
                AddRecentCase(caseInfo.RootPath);
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
                AddRecentCase(folder);
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

        public async Task OpenCaseFromPathAsync(string? path)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                IsBusy = true;
                var caseInfo = await _caseStorage.LoadCaseAsync(path);
                await LoadCaseDataAsync(caseInfo);
                AddRecentCase(path);
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
            _caseSettings = new CaseSettings();
            _isLoadingCaseSettings = true;

            EvidenceTypes.Clear();
            People.Clear();
            _typeLookup.Clear();
            _peopleLookup.Clear();
            _allEvidenceSummaries.Clear();
            _evidenceById.Clear();
            PersonOptions.Clear();

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
                AddPersonOption(person.Id, person.Name);
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

            await LoadCaseSettingsAsync(caseInfo);
            ApplyCaseSettings();
            _isLoadingCaseSettings = false;
        }

        private (string? root, string? name, string? number) PromptForCaseDetails()
        {
            var root = UIHelpers.PickFolder("Select a folder to create the case in");
            if (string.IsNullOrWhiteSpace(root))
            {
                return (null, null, null);
            }

            var name = UIHelpers.PromptForText("New Case", "Enter case name:");
            if (string.IsNullOrWhiteSpace(name))
            {
                return (null, null, null);
            }

            var number = UIHelpers.PromptForText("New Case", "Enter case number (optional):") ?? string.Empty;
            return (root, name, number);
        }

        private EvidenceSummary BuildSummary(Evidence evidence)
        {
            var typeName = _typeLookup.TryGetValue(evidence.TypeId, out var type) ? type.Name : string.Empty;
            var personNames = evidence.PersonIds.Select(id => _peopleLookup.TryGetValue(id, out var person) ? person.Name : id).ToArray();
            var personAliases = evidence.PersonIds
                .SelectMany(id => _peopleLookup.TryGetValue(id, out var person) && person.Aliases != null
                    ? person.Aliases
                    : Enumerable.Empty<string>())
                .ToArray();

            return new EvidenceSummary
            {
                Id = evidence.Id,
                EvidenceNumber = evidence.EvidenceNumber,
                Title = evidence.Title,
                CourtNumber = evidence.CourtNumber,
                TypeName = typeName,
                PersonNames = personNames,
                DateDisplay = ResolveDateDisplay(evidence.DateInfo),
                SortDate = evidence.DateInfo.SortDate.ToDateTime(TimeOnly.MinValue),
                SearchKey = BuildSearchKey(evidence.Title, evidence.CourtNumber, typeName, personNames, personAliases)
            };
        }

        private static string BuildSearchKey(string title, string courtNumber, string typeName, IEnumerable<string> personNames, IEnumerable<string> personAliases)
        {
            var parts = new List<string>
            {
                title,
                courtNumber,
                typeName
            };
            parts.AddRange(personNames);
            parts.AddRange(personAliases);
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

        public void ApplyFilters()
        {
            var filtered = _allEvidenceSummaries.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var needle = SearchText.ToLowerInvariant();
                filtered = filtered.Where(s => s.SearchKey.Contains(needle, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedType != null)
            {
                filtered = filtered.Where(s => string.Equals(s.TypeName, SelectedType.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedPerson != null)
            {
                filtered = filtered.Where(s => s.PersonNames.Any(p => string.Equals(p, SelectedPerson.Name, StringComparison.OrdinalIgnoreCase)));
            }

            filtered = _sortNewestFirst
                ? filtered.OrderByDescending(s => s.SortDate).ThenBy(s => s.EvidenceNumber)
                : filtered.OrderBy(s => s.SortDate).ThenBy(s => s.EvidenceNumber);

            EvidenceList.Clear();
            foreach (var item in filtered)
            {
                EvidenceList.Add(item);
            }
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedType = null;
            SelectedPerson = null;
            _ = SaveCaseSettingsAsync();
        }

        private async Task NewEvidenceAsync()
        {
            if (CurrentCase == null)
            {
                return;
            }

            var newEvidence = new Evidence
            {
                Title = "New Evidence",
                EvidenceNumber = CurrentCase.NextEvidenceNumber,
                TypeId = EvidenceTypes.FirstOrDefault()?.Id ?? string.Empty,
                DateInfo = new EvidenceDateInfo
                {
                    Mode = EvidenceDateMode.Exact,
                    SortDate = DateOnly.FromDateTime(DateTime.UtcNow)
                },
                Attachments = new List<AttachmentInfo>(),
                PersonIds = new List<string>(),
                LinkedEvidenceIds = new List<string>(),
                NoteFile = "note.md"
            };

            var vm = new EvidenceWindowViewModel(CurrentCase, newEvidence, _evidenceStorage, _referenceData, isNew: true);
            vm.EvidenceSaved += OnExternalEvidenceSaved;
            vm.NotesSaved += OnExternalNotesSaved;

            var window = new EvidenceWindow
            {
                Owner = WpfApp.Current?.MainWindow,
                DataContext = vm
            };

            window.Closed += (_, _) =>
            {
                vm.EvidenceSaved -= OnExternalEvidenceSaved;
                vm.NotesSaved -= OnExternalNotesSaved;
            };

            _ = vm.LoadAsync();
            window.Show();
        }

        private async Task LoadSelectedEvidenceAsync(EvidenceSummary? summary)
        {
            _notesAutoSaveCts?.Cancel();
            _metadataAutoSaveCts?.Cancel();

            if (summary == null || CurrentCase == null)
            {
                SelectedEvidenceDetail = null;
                _suppressNotesAutoSave = true;
                try
                {
                    NotesText = string.Empty;
                }
                finally
                {
                    _suppressNotesAutoSave = false;
                }
                return;
            }

            if (_evidenceById.TryGetValue(summary.Id, out var evidence))
            {
                SelectedEvidenceDetail = evidence;
                SyncSelectionOptions(evidence);
                await LoadNotesAsync(evidence);
                _loadedEvidenceSnapshot = CloneEvidence(evidence);
                _loadedNotesSnapshot = NotesText;
            }
            else
            {
                SelectedEvidenceDetail = null;
                _suppressNotesAutoSave = true;
                try
                {
                    NotesText = string.Empty;
                }
                finally
                {
                _suppressNotesAutoSave = false;
                }
                SyncSelectionOptions(null);
                _loadedEvidenceSnapshot = null;
                _loadedNotesSnapshot = string.Empty;
            }
        }

        private async Task LoadNotesAsync(Evidence evidence)
        {
            if (CurrentCase == null)
            {
                NotesText = string.Empty;
                return;
            }

            _suppressNotesAutoSave = true;
            try
            {
                var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(CurrentCase, evidence);
                var noteFile = evidence.NoteFile ?? "note.md";
                var notePath = Path.Combine(folder, noteFile);

                if (File.Exists(notePath))
                {
                    NotesText = await File.ReadAllTextAsync(notePath);
                    _loadedNotesSnapshot = NotesText;
                }
                else
                {
                    NotesText = string.Empty;
                    _loadedNotesSnapshot = string.Empty;
                }
            }
            finally
            {
                _suppressNotesAutoSave = false;
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
                var currentNotes = NotesText ?? string.Empty;
                if (string.Equals(currentNotes, _loadedNotesSnapshot ?? string.Empty, StringComparison.Ordinal))
                {
                    return;
                }

                var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(CurrentCase, SelectedEvidenceDetail);
                var noteFile = SelectedEvidenceDetail.NoteFile ?? "note.md";
                var notePath = Path.Combine(folder, noteFile);
                Directory.CreateDirectory(folder);
                await File.WriteAllTextAsync(notePath, currentNotes);
                _loadedNotesSnapshot = currentNotes;
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

        private void OpenPreferences()
        {
            _appSettings ??= new AppSettings();
            var vm = new PreferencesViewModel(_appSettings);
            var window = new PreferencesWindow
            {
                Owner = WpfApp.Current?.MainWindow,
                DataContext = vm
            };

            var result = window.ShowDialog();
            if (result == true)
            {
                _appSettings = vm.ToAppSettings();
                ApplyThemePreference();
                _ = SaveAppSettingsAsync();
            }
        }

        private void SetZoom(object? parameter)
        {
            if (parameter is double zoom)
            {
                ZoomLevel = zoom;
            }
            else if (parameter is string zoomStr && double.TryParse(zoomStr, out var zoomValue))
            {
                ZoomLevel = zoomValue;
            }
        }

        private void SetEditorFontSize(object? parameter)
        {
            if (parameter is double fontSize)
            {
                EditorFontSize = fontSize;
            }
            else if (parameter is string fontSizeStr && double.TryParse(fontSizeStr, out var fontSizeValue))
            {
                EditorFontSize = fontSizeValue;
            }
        }

        private async Task OpenCaseSettingsAsync()
        {
            if (CurrentCase == null)
            {
                return;
            }

            var originalTypes = EvidenceTypes.ToList();
            var originalPeople = People.ToList();

            var vm = new CaseSettingsViewModel(_caseSettings, originalTypes, originalPeople);
            var window = new CaseSettingsWindow
            {
                Owner = WpfApp.Current?.MainWindow,
                DataContext = vm
            };

            var result = window.ShowDialog();
            if (result == true)
            {
                _isLoadingCaseSettings = true;
                try
                {
                    IsLeftPaneVisible = vm.ShowLeftPane;
                    IsRightPaneVisible = vm.ShowRightPane;
                    IsBottomPaneVisible = vm.ShowBottomPane;
                    SortNewestFirst = vm.SortNewestFirst;
                    _caseSettings = vm.ToCaseSettings();
                }
                finally
                {
                    _isLoadingCaseSettings = false;
                }

                try
                {
                    if (vm.HasTypeChanges)
                    {
                        await ApplyTypeChangesAsync(vm.EvidenceTypes.Select(t => t.ToEvidenceType()).ToList(), originalTypes);
                    }

                    if (vm.HasPeopleChanges)
                    {
                        await ApplyPeopleChangesAsync(vm.People.Select(p => p.ToPerson()).ToList(), originalPeople);
                    }

                    _ = SaveCaseSettingsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to save case settings: {ex.Message}", "Case Settings", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SyncSelectionOptions(Evidence? evidence)
        {
            _suppressMetadataAutoSave = true;
            try
            {
                foreach (var option in PersonOptions)
                {
                    option.IsSelected = evidence != null && evidence.PersonIds.Any(id => string.Equals(id, option.Id, StringComparison.OrdinalIgnoreCase));
                }
            }
            finally
            {
                _suppressMetadataAutoSave = false;
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
                SelectedEvidenceDetail.PersonIds = PersonOptions.Where(o => o.IsSelected).Select(o => o.Id).ToList();
                SelectedEvidenceDetail.LinkedEvidenceIds = ParseLinkedEvidenceIds(LinkedEvidenceText);

                UpdateSortDate(SelectedEvidenceDetail);

                if (_loadedEvidenceSnapshot != null && !HasMetadataChanges(SelectedEvidenceDetail, _loadedEvidenceSnapshot))
                {
                    return;
                }

                await _evidenceStorage.SaveEvidenceAsync(CurrentCase, SelectedEvidenceDetail);

                var updatedSummary = BuildSummary(SelectedEvidenceDetail);
                UpdateSummaryLists(updatedSummary);
                _loadedEvidenceSnapshot = CloneEvidence(SelectedEvidenceDetail);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save metadata: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RequestMetadataAutoSave(bool immediate = false)
        {
            if (_suppressMetadataAutoSave || CurrentCase == null || SelectedEvidenceDetail == null)
            {
                return;
            }

            if (immediate)
            {
                _ = SaveMetadataAsync();
                return;
            }

            _metadataAutoSaveCts?.Cancel();
            _metadataAutoSaveCts = new CancellationTokenSource();
            var token = _metadataAutoSaveCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_metadataAutoSaveDelay, token);
                    token.ThrowIfCancellationRequested();
                    if (WpfApp.Current != null)
                    {
                        await WpfApp.Current.Dispatcher.InvokeAsync(async () => await SaveMetadataAsync());
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    if (WpfApp.Current != null)
                    {
                        await WpfApp.Current.Dispatcher.InvokeAsync(() =>
                            MessageBox.Show($"Unable to autosave metadata: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                    }
                }
            });
        }

        private void QueueNotesAutoSave()
        {
            if (_suppressNotesAutoSave || CurrentCase == null || SelectedEvidenceDetail == null)
            {
                return;
            }

            _notesAutoSaveCts?.Cancel();
            _notesAutoSaveCts = new CancellationTokenSource();
            var token = _notesAutoSaveCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_notesAutoSaveDelay, token);
                    token.ThrowIfCancellationRequested();
                    if (WpfApp.Current != null)
                    {
                        await WpfApp.Current.Dispatcher.InvokeAsync(async () => await SaveNotesAsync());
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    if (WpfApp.Current != null)
                    {
                        await WpfApp.Current.Dispatcher.InvokeAsync(() =>
                            MessageBox.Show($"Unable to autosave notes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                    }
                }
            });
        }

        private void UpdateSummaryLists(EvidenceSummary updated, string? preferredSelectionId = null)
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
            var targetId = preferredSelectionId ?? updated.Id;
            var selection = EvidenceList.FirstOrDefault(s => string.Equals(s.Id, targetId, StringComparison.OrdinalIgnoreCase))
                ?? EvidenceList.FirstOrDefault();
            SelectedSummary = selection;
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

        private void OpenEvidenceWindow()
        {
            if (SelectedSummary == null || CurrentCase == null)
            {
                return;
            }

            TryOpenEvidenceWindow(SelectedSummary.Id, WpfApp.Current?.MainWindow);
        }

        private async Task DeleteEvidenceAsync()
        {
            if (CurrentCase == null || SelectedSummary == null)
            {
                return;
            }

            var label = $"#{SelectedSummary.EvidenceNumber} {SelectedSummary.Title}".Trim();
            var confirm = MessageBox.Show(
                $"Are you sure you want to delete evidence {label}? This cannot be undone.",
                "Delete Evidence",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await _evidenceStorage.DeleteEvidenceAsync(CurrentCase, SelectedSummary.Id);

                _allEvidenceSummaries.RemoveAll(s => string.Equals(s.Id, SelectedSummary.Id, StringComparison.OrdinalIgnoreCase));
                var displayItem = EvidenceList.FirstOrDefault(s => string.Equals(s.Id, SelectedSummary.Id, StringComparison.OrdinalIgnoreCase));
                if (displayItem != null)
                {
                    EvidenceList.Remove(displayItem);
                }

                SelectedSummary = EvidenceList.FirstOrDefault();
                SelectedEvidenceDetail = null;
                _loadedEvidenceSnapshot = null;
                _loadedNotesSnapshot = string.Empty;

                RaiseCommandStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to delete evidence: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            if (DeleteEvidenceCommand is AsyncRelayCommand asyncDeleteEvidence)
            {
                asyncDeleteEvidence.RaiseCanExecuteChanged();
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

            if (AddAttachmentCommand is AsyncRelayCommand asyncAddAttachment) asyncAddAttachment.RaiseCanExecuteChanged();
            if (RemoveAttachmentCommand is RelayCommand<AttachmentInfo> relayRemoveAttachment) relayRemoveAttachment.RaiseCanExecuteChanged();
            if (AddTypeCommand is AsyncRelayCommand asyncAddType) asyncAddType.RaiseCanExecuteChanged();
            if (RenameTypeCommand is AsyncRelayCommand asyncRenameType) asyncRenameType.RaiseCanExecuteChanged();
            if (DeleteTypeCommand is AsyncRelayCommand asyncDeleteType) asyncDeleteType.RaiseCanExecuteChanged();
            if (AddPersonCommand is AsyncRelayCommand asyncAddPerson) asyncAddPerson.RaiseCanExecuteChanged();
            if (RenamePersonCommand is AsyncRelayCommand asyncRenamePerson) asyncRenamePerson.RaiseCanExecuteChanged();
            if (DeletePersonCommand is AsyncRelayCommand asyncDeletePerson) asyncDeletePerson.RaiseCanExecuteChanged();
            if (OpenCaseSettingsCommand is AsyncRelayCommand asyncCaseSettings) asyncCaseSettings.RaiseCanExecuteChanged();
        }

        private async Task AddAttachmentAsync()
        {
            if (CurrentCase == null || SelectedEvidenceDetail == null)
            {
                return;
            }

            using var dialog = new WinForms.OpenFileDialog
            {
                Title = "Select attachment(s)",
                Multiselect = true
            };

            var result = dialog.ShowDialog();
            if (result != WinForms.DialogResult.OK || dialog.FileNames.Length == 0)
            {
                return;
            }

            await AddAttachmentsFromPathsAsync(dialog.FileNames);
        }

        public async Task AddAttachmentsFromPathsAsync(IEnumerable<string> filePaths)
        {
            if (CurrentCase == null || SelectedEvidenceDetail == null)
            {
                return;
            }

            await AddAttachmentsToEvidenceAsync(CurrentCase, SelectedEvidenceDetail, filePaths);
        }

        private async Task OpenAttachmentAsync(AttachmentInfo? attachment)
        {
            if (CurrentCase == null || SelectedEvidenceDetail == null || attachment == null)
            {
                return;
            }

            var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(CurrentCase, SelectedEvidenceDetail);
            var fullPath = Path.Combine(folder, attachment.RelativePath);
            if (!File.Exists(fullPath))
            {
                MessageBox.Show($"File not found: {attachment.FileName}", "Open Attachment", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                OpenWithDefaultApp(fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open attachment: {ex.Message}", "Open Attachment", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OpenAttachmentFolderAsync(AttachmentInfo? attachment)
        {
            if (CurrentCase == null || SelectedEvidenceDetail == null || attachment == null)
            {
                return;
            }

            var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(CurrentCase, SelectedEvidenceDetail);
            var fullPath = Path.Combine(folder, attachment.RelativePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                MessageBox.Show("Attachment folder not found.", "Open Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = directory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open folder: {ex.Message}", "Open Folder", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RemoveAttachmentAsync(AttachmentInfo? attachment)
        {
            if (CurrentCase == null || SelectedEvidenceDetail == null)
            {
                return;
            }

            var target = attachment ?? SelectedEvidenceDetail.Attachments.LastOrDefault();
            if (target == null)
            {
                return;
            }

            var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(CurrentCase, SelectedEvidenceDetail);
            var fullPath = Path.Combine(folder, target.RelativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            SelectedEvidenceDetail.Attachments.Remove(target);
            await _evidenceStorage.SaveEvidenceAsync(CurrentCase, SelectedEvidenceDetail);
            var updatedSummary = BuildSummary(SelectedEvidenceDetail);
            UpdateSummaryLists(updatedSummary);
            _loadedEvidenceSnapshot = CloneEvidence(SelectedEvidenceDetail);
        }

        private static void OpenWithDefaultApp(string path)
        {
            var extension = Path.GetExtension(path);
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                Verb = "open"
            };

            if (!string.IsNullOrWhiteSpace(extension) && HtmlExtensions.Contains(extension))
            {
                startInfo.FileName = new Uri(path).AbsoluteUri;
            }
            else
            {
                startInfo.FileName = path;
            }

            Process.Start(startInfo);
        }

        private static string EnsureUniqueFilePath(string basePath)
        {
            if (!File.Exists(basePath))
            {
                return basePath;
            }

            var directory = Path.GetDirectoryName(basePath) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(basePath);
            var extension = Path.GetExtension(basePath);
            var counter = 1;

            string candidate;
            do
            {
                candidate = Path.Combine(directory, $"{name}_{counter}{extension}");
                counter++;
            } while (File.Exists(candidate));

            return candidate;
        }

        private static readonly HashSet<string> HtmlExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".html",
            ".htm",
            ".xhtml",
            ".mht",
            ".mhtml"
        };

        private async Task AddAttachmentsToEvidenceAsync(CaseInfo caseInfo, Evidence evidence, IEnumerable<string> filePaths)
        {
            var files = filePaths
                .Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (files.Count == 0)
            {
                return;
            }

            var targetFolder = await _evidenceStorage.GetEvidenceFolderPathAsync(caseInfo, evidence);
            var filesFolder = Path.Combine(targetFolder, "files");
            Directory.CreateDirectory(filesFolder);

            var added = false;
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var targetPath = Path.Combine(filesFolder, fileName);
                targetPath = EnsureUniqueFilePath(targetPath);
                File.Copy(file, targetPath, true);

                var relative = Path.Combine("files", Path.GetFileName(targetPath));
                if (evidence.Attachments.All(a => !string.Equals(a.RelativePath, relative, StringComparison.OrdinalIgnoreCase)))
                {
                    evidence.Attachments.Add(new AttachmentInfo
                    {
                        FileName = Path.GetFileName(targetPath),
                        RelativePath = relative
                    });
                    added = true;
                }
            }

            if (!added)
            {
                return;
            }

            await _evidenceStorage.SaveEvidenceAsync(caseInfo, evidence);
            var updatedSummary = BuildSummary(evidence);
            UpdateSummaryLists(updatedSummary, evidence.Id);

            if (SelectedEvidenceDetail != null && string.Equals(SelectedEvidenceDetail.Id, evidence.Id, StringComparison.OrdinalIgnoreCase))
            {
                _loadedEvidenceSnapshot = CloneEvidence(SelectedEvidenceDetail);
            }
        }

        private void ManageLinks()
        {
            if (CurrentCase == null || SelectedEvidenceDetail == null)
            {
                return;
            }

            var available = _allEvidenceSummaries.Where(e => !string.Equals(e.Id, SelectedEvidenceDetail.Id, StringComparison.OrdinalIgnoreCase)).ToList();
            var dialog = new LinkEvidenceDialog(available, SelectedEvidenceDetail.LinkedEvidenceIds)
            {
                Owner = WpfApp.Current?.MainWindow
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                _suppressMetadataAutoSave = true;
                try
                {
                    SelectedEvidenceDetail.LinkedEvidenceIds = dialog.SelectedIds;
                    LinkedEvidenceText = string.Join(", ", dialog.SelectedIds);
                }
                finally
                {
                    _suppressMetadataAutoSave = false;
                }

                RequestMetadataAutoSave(true);
            }
        }

        private async Task AddTypeAsync()
        {
            if (CurrentCase == null)
            {
                return;
            }

            var name = UIHelpers.PromptForText("Add Type", "Enter type name:");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var type = new EvidenceType { Id = Guid.NewGuid().ToString("N"), Name = name.Trim() };
            EvidenceTypes.Add(type);
            _typeLookup[type.Id] = type;
            await _referenceData.SaveTypesAsync(CurrentCase, EvidenceTypes.ToList());
            RebuildSummaries();
        }

        private async Task RenameTypeAsync()
        {
            if (CurrentCase == null || SelectedType == null)
            {
                return;
            }

            var newName = UIHelpers.PromptForText("Rename Type", "Enter new type name:", SelectedType.Name);
            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            SelectedType.Name = newName.Trim();
            _typeLookup[SelectedType.Id] = SelectedType;
            await _referenceData.SaveTypesAsync(CurrentCase, EvidenceTypes.ToList());
            RebuildSummaries();
        }

        private async Task DeleteTypeAsync()
        {
            if (CurrentCase == null || SelectedType == null)
            {
                return;
            }

            var type = SelectedType;
            EvidenceTypes.Remove(type);
            _typeLookup.Remove(type.Id);

            foreach (var evidence in _evidenceById.Values)
            {
                if (string.Equals(evidence.TypeId, type.Id, StringComparison.OrdinalIgnoreCase))
                {
                    evidence.TypeId = string.Empty;
                    await _evidenceStorage.SaveEvidenceAsync(CurrentCase, evidence);
                }
            }

            await _referenceData.SaveTypesAsync(CurrentCase, EvidenceTypes.ToList());
            RebuildSummaries();
            SelectedType = null;
        }

        private async Task AddPersonAsync()
        {
            if (CurrentCase == null)
            {
                return;
            }

            var name = UIHelpers.PromptForText("Add Person", "Enter person name:");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var person = new Person { Id = Guid.NewGuid().ToString("N"), Name = name.Trim() };
            People.Add(person);
            AddPersonOption(person.Id, person.Name);
            _peopleLookup[person.Id] = person;
            await _referenceData.SavePeopleAsync(CurrentCase, People.ToList());
            RebuildSummaries();
        }

        private async Task RenamePersonAsync()
        {
            if (CurrentCase == null || SelectedPerson == null)
            {
                return;
            }

            var newName = UIHelpers.PromptForText("Rename Person", "Enter new person name:", SelectedPerson.Name);
            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            SelectedPerson.Name = newName.Trim();
            var option = PersonOptions.FirstOrDefault(o => string.Equals(o.Id, SelectedPerson.Id, StringComparison.OrdinalIgnoreCase));
            if (option != null)
            {
                option.Name = SelectedPerson.Name;
            }

            _peopleLookup[SelectedPerson.Id] = SelectedPerson;
            await _referenceData.SavePeopleAsync(CurrentCase, People.ToList());
            RebuildSummaries();
        }

        private async Task DeletePersonAsync()
        {
            if (CurrentCase == null || SelectedPerson == null)
            {
                return;
            }

            var person = SelectedPerson;
            People.Remove(person);
            var option = PersonOptions.FirstOrDefault(o => string.Equals(o.Id, person.Id, StringComparison.OrdinalIgnoreCase));
            if (option != null)
            {
                PersonOptions.Remove(option);
            }
            _peopleLookup.Remove(person.Id);

            foreach (var evidence in _evidenceById.Values)
            {
                if (evidence.PersonIds.Remove(person.Id))
                {
                    await _evidenceStorage.SaveEvidenceAsync(CurrentCase, evidence);
                }
            }

            await _referenceData.SavePeopleAsync(CurrentCase, People.ToList());
            RebuildSummaries();
            SelectedPerson = null;
        }

        private void RebuildSummaries()
        {
            _allEvidenceSummaries.Clear();
            foreach (var ev in _evidenceById.Values)
            {
                _allEvidenceSummaries.Add(BuildSummary(ev));
            }
            ApplyFilters();
        }

        private async Task ApplyTypeChangesAsync(List<EvidenceType> updatedTypes, List<EvidenceType> previousTypes)
        {
            if (CurrentCase == null)
            {
                return;
            }

            var removedIds = new HashSet<string>(previousTypes.Select(t => t.Id), StringComparer.OrdinalIgnoreCase);
            removedIds.ExceptWith(updatedTypes.Select(t => t.Id));

            var previousSelectedId = SelectedType?.Id;

            EvidenceTypes.Clear();
            _typeLookup.Clear();
            foreach (var type in updatedTypes)
            {
                EvidenceTypes.Add(type);
                _typeLookup[type.Id] = type;
            }

            foreach (var evidence in _evidenceById.Values)
            {
                if (removedIds.Contains(evidence.TypeId))
                {
                    evidence.TypeId = string.Empty;
                    await _evidenceStorage.SaveEvidenceAsync(CurrentCase, evidence);
                }
            }

            await _referenceData.SaveTypesAsync(CurrentCase, updatedTypes);

            SelectedType = EvidenceTypes.FirstOrDefault(t => string.Equals(t.Id, previousSelectedId, StringComparison.OrdinalIgnoreCase));
            RebuildSummaries();
        }

        private async Task ApplyPeopleChangesAsync(List<Person> updatedPeople, List<Person> previousPeople)
        {
            if (CurrentCase == null)
            {
                return;
            }

            var removedIds = new HashSet<string>(previousPeople.Select(p => p.Id), StringComparer.OrdinalIgnoreCase);
            removedIds.ExceptWith(updatedPeople.Select(p => p.Id));

            var previousSelectedPersonId = SelectedPerson?.Id;

            People.Clear();
            _peopleLookup.Clear();
            foreach (var person in updatedPeople)
            {
                People.Add(person);
                _peopleLookup[person.Id] = person;
            }

            RebuildPersonOptions();

            foreach (var evidence in _evidenceById.Values)
            {
                var beforeCount = evidence.PersonIds.Count;
                evidence.PersonIds = evidence.PersonIds
                    .Where(id => !removedIds.Contains(id))
                    .ToList();

                if (evidence.PersonIds.Count != beforeCount)
                {
                    await _evidenceStorage.SaveEvidenceAsync(CurrentCase, evidence);
                }
            }

            await _referenceData.SavePeopleAsync(CurrentCase, updatedPeople);

            SelectedPerson = People.FirstOrDefault(p => string.Equals(p.Id, previousSelectedPersonId, StringComparison.OrdinalIgnoreCase));
            RebuildSummaries();
            SyncSelectionOptions(SelectedEvidenceDetail);
        }

        private void RebuildPersonOptions()
        {
            _suppressMetadataAutoSave = true;
            try
            {
                PersonOptions.Clear();
                foreach (var person in People)
                {
                    AddPersonOption(person.Id, person.Name);
                }
            }
            finally
            {
                _suppressMetadataAutoSave = false;
            }
        }

        private void OnPersonOptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<SelectableItem>())
                {
                    item.PropertyChanged += OnOptionPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<SelectableItem>())
                {
                    item.PropertyChanged -= OnOptionPropertyChanged;
                }
            }
        }

        private void OnOptionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(SelectableItem.IsSelected), StringComparison.Ordinal))
            {
                RequestMetadataAutoSave();
            }
        }

        private static Evidence CloneEvidence(Evidence source)
        {
            return new Evidence
            {
                Id = source.Id,
                EvidenceNumber = source.EvidenceNumber,
                Title = source.Title,
                CourtNumber = source.CourtNumber,
                TypeId = source.TypeId,
                PersonIds = new List<string>(source.PersonIds),
                LinkedEvidenceIds = new List<string>(source.LinkedEvidenceIds),
                NoteFile = source.NoteFile,
                Attachments = new List<AttachmentInfo>(source.Attachments.Select(a => new AttachmentInfo
                {
                    FileName = a.FileName,
                    RelativePath = a.RelativePath
                })),
                DateInfo = new EvidenceDateInfo
                {
                    Mode = source.DateInfo.Mode,
                    ExactDate = source.DateInfo.ExactDate,
                    StartDate = source.DateInfo.StartDate,
                    EndDate = source.DateInfo.EndDate,
                    AroundAmount = source.DateInfo.AroundAmount,
                    AroundUnit = source.DateInfo.AroundUnit,
                    SortDate = source.DateInfo.SortDate
                },
                CreatedAt = source.CreatedAt,
                LastModifiedAt = source.LastModifiedAt
            };
        }

        private static bool HasMetadataChanges(Evidence current, Evidence snapshot)
        {
            if (snapshot == null)
            {
                return false;
            }

            if (!string.Equals(current.Title, snapshot.Title, StringComparison.Ordinal) ||
                !string.Equals(current.CourtNumber, snapshot.CourtNumber, StringComparison.Ordinal) ||
                !string.Equals(current.TypeId, snapshot.TypeId, StringComparison.Ordinal))
            {
                return true;
            }

            if (!ListMatches(current.PersonIds, snapshot.PersonIds) ||
                !ListMatches(current.LinkedEvidenceIds, snapshot.LinkedEvidenceIds))
            {
                return true;
            }

            if (current.DateInfo.Mode != snapshot.DateInfo.Mode ||
                current.DateInfo.ExactDate != snapshot.DateInfo.ExactDate ||
                current.DateInfo.StartDate != snapshot.DateInfo.StartDate ||
                current.DateInfo.EndDate != snapshot.DateInfo.EndDate ||
                current.DateInfo.AroundAmount != snapshot.DateInfo.AroundAmount ||
                !string.Equals(current.DateInfo.AroundUnit, snapshot.DateInfo.AroundUnit, StringComparison.Ordinal))
            {
                return true;
            }

            if (!AttachmentsMatch(current.Attachments, snapshot.Attachments))
            {
                return true;
            }

            return false;
        }

        private static bool ListMatches(IList<string> current, IList<string> snapshot)
        {
            if (current.Count != snapshot.Count)
            {
                return false;
            }

            return current.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .SequenceEqual(snapshot.OrderBy(x => x, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        }

        private static bool AttachmentsMatch(IList<AttachmentInfo> current, IList<AttachmentInfo> snapshot)
        {
            if (current.Count != snapshot.Count)
            {
                return false;
            }

            var currentKeys = current.Select(a => $"{a.RelativePath}|{a.FileName}")
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
            var snapshotKeys = snapshot.Select(a => $"{a.RelativePath}|{a.FileName}")
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

            return currentKeys.SequenceEqual(snapshotKeys, StringComparer.OrdinalIgnoreCase);
        }

        private void AddPersonOption(string id, string name)
        {
            var option = new SelectableItem(id, name);
            option.PropertyChanged += OnOptionPropertyChanged;
            PersonOptions.Add(option);
        }

        private async Task LoadAppSettingsAsync()
        {
            try
            {
                var path = GetAppSettingsPath();
                if (File.Exists(path))
                {
                    var settings = await JsonHelper.LoadAsync<AppSettings>(path);
                    _appSettings = settings ?? new AppSettings();
                }
                ApplyThemePreference();
            }
            catch
            {
                _appSettings = new AppSettings();
            }
        }

        private async Task SaveAppSettingsAsync()
        {
            try
            {
                var path = GetAppSettingsPath();
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await JsonHelper.SaveAsync(path, _appSettings);
            }
            catch
            {
            }
        }

        private void ApplyThemePreference()
        {
            try
            {
                if (WpfApp.Current is App app)
                {
                    app.ApplyTheme(_appSettings?.Theme);
                }
            }
            catch
            {
                // Theme application is best-effort; ignore failures to keep startup resilient.
            }
        }

        private static string GetAppSettingsPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#if DEBUG
            var folder = Path.Combine(appData, "EvidenceTimeline-Dev");
#else
            var folder = Path.Combine(appData, "EvidenceTimeline");
#endif
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "settings.json");
        }

        private void AddRecentCase(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            _appSettings ??= new AppSettings();
            _appSettings.RecentCases.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            _appSettings.RecentCases.Insert(0, path);
            if (_appSettings.RecentCases.Count > 10)
            {
                _appSettings.RecentCases = _appSettings.RecentCases.Take(10).ToList();
            }

            _ = SaveAppSettingsAsync();
        }

        private async Task LoadCaseSettingsAsync(CaseInfo caseInfo)
        {
            try
            {
                var path = GetCaseSettingsPath(caseInfo);
                if (File.Exists(path))
                {
                    var loaded = await JsonHelper.LoadAsync<CaseSettings>(path);
                    if (loaded != null)
                    {
                        _caseSettings = loaded;
                    }
                }
            }
            catch
            {
                _caseSettings = new CaseSettings();
            }
        }

        private async Task SaveCaseSettingsAsync()
        {
            if (CurrentCase == null || _isLoadingCaseSettings)
            {
                return;
            }

            try
            {
                _caseSettings.ShowLeftPane = IsLeftPaneVisible;
                _caseSettings.ShowRightPane = IsRightPaneVisible;
                _caseSettings.ShowBottomPane = IsBottomPaneVisible;
                _caseSettings.SortNewestFirst = SortNewestFirst;
                _caseSettings.ZoomLevel = ZoomLevel;
                _caseSettings.EditorFontSize = EditorFontSize;

                var path = GetCaseSettingsPath(CurrentCase);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await JsonHelper.SaveAsync(path, _caseSettings);
            }
            catch
            {
            }
        }

        private void ApplyCaseSettings()
        {
            _isLoadingCaseSettings = true;
            try
            {
                IsLeftPaneVisible = _caseSettings.ShowLeftPane;
                IsRightPaneVisible = _caseSettings.ShowRightPane;
                IsBottomPaneVisible = _caseSettings.ShowBottomPane;
                SortNewestFirst = _caseSettings.SortNewestFirst;
                ZoomLevel = _caseSettings.ZoomLevel;
                EditorFontSize = _caseSettings.EditorFontSize;
            }
            finally
            {
                _isLoadingCaseSettings = false;
            }
        }

        private static string GetCaseSettingsPath(CaseInfo caseInfo)
        {
            if (string.IsNullOrWhiteSpace(caseInfo.RootPath))
            {
                throw new InvalidOperationException("Case root path is required for settings.");
            }

            return Path.Combine(caseInfo.RootPath, "caseSettings.json");
        }
        private async void OnExternalEvidenceSaved(object? sender, Evidence updated)
        {
            if (CurrentCase == null)
            {
                return;
            }

            try
            {
                if (WpfApp.Current?.Dispatcher.CheckAccess() == true)
                {
                    HandleExternalEvidenceSave(updated);
                }
                else
                {
                    await WpfApp.Current!.Dispatcher.InvokeAsync(() => HandleExternalEvidenceSave(updated));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to refresh evidence after external save: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleExternalEvidenceSave(Evidence updated)
        {
            var isSelected = SelectedEvidenceDetail != null
                && string.Equals(SelectedEvidenceDetail.Id, updated.Id, StringComparison.OrdinalIgnoreCase);
            var hasLocalChanges = isSelected
                && _loadedEvidenceSnapshot != null
                && HasMetadataChanges(SelectedEvidenceDetail!, _loadedEvidenceSnapshot);

            if (isSelected && hasLocalChanges)
            {
                var result = MessageBox.Show(
                    "This evidence was updated in another window. Reload their changes and discard your unsaved edits?",
                    "Evidence Updated",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            UpdateSortDate(updated);
            _evidenceById[updated.Id] = updated;

            if (CurrentCase != null)
            {
                CurrentCase.NextEvidenceNumber = Math.Max(CurrentCase.NextEvidenceNumber, updated.EvidenceNumber + 1);
            }

            if (isSelected)
            {
                SelectedEvidenceDetail = updated;
                SyncSelectionOptions(updated);
                _loadedEvidenceSnapshot = CloneEvidence(updated);
                LinkedEvidenceText = string.Join(", ", updated.LinkedEvidenceIds);
            }

            RebuildSummaries();
            SelectedSummary = EvidenceList.FirstOrDefault(s => string.Equals(s.Id, updated.Id, StringComparison.OrdinalIgnoreCase))
                ?? SelectedSummary;
        }

        private async void OnExternalNotesSaved(object? sender, string evidenceId)
        {
            if (CurrentCase == null || SelectedEvidenceDetail == null)
            {
                return;
            }

            if (!string.Equals(SelectedEvidenceDetail.Id, evidenceId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var hasLocalChanges = !string.Equals(NotesText ?? string.Empty, _loadedNotesSnapshot ?? string.Empty, StringComparison.Ordinal);
            if (hasLocalChanges)
            {
                var result = MessageBox.Show(
                    "Notes were updated in another window. Reload their changes and discard your unsaved edits?",
                    "Notes Updated",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            try
            {
                if (WpfApp.Current?.Dispatcher.CheckAccess() == true)
                {
                    _ = ReloadNotesAsync();
                }
                else
                {
                    await WpfApp.Current!.Dispatcher.InvokeAsync(() => _ = ReloadNotesAsync());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to refresh notes after external save: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ReloadNotesAsync()
        {
            if (CurrentCase == null || SelectedEvidenceDetail == null)
            {
                return;
            }

            try
            {
                await LoadNotesAsync(SelectedEvidenceDetail);
            }
            finally
            {
                _loadedNotesSnapshot = NotesText;
            }
        }

        public void TryOpenEvidenceWindow(string evidenceId, Window? owner)
        {
            if (CurrentCase == null || !_evidenceById.TryGetValue(evidenceId, out var evidence))
            {
                return;
            }

            var vm = new EvidenceWindowViewModel(CurrentCase, evidence, _evidenceStorage, _referenceData);
            vm.EvidenceSaved += OnExternalEvidenceSaved;
            vm.NotesSaved += OnExternalNotesSaved;

            var window = new EvidenceWindow
            {
                Owner = owner,
                DataContext = vm
            };

            window.Closed += (_, _) =>
            {
                vm.EvidenceSaved -= OnExternalEvidenceSaved;
                vm.NotesSaved -= OnExternalNotesSaved;
            };

            _ = vm.LoadAsync();
            window.Show();
        }
    }
}
