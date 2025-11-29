using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using evidence_timeline.Models;
using evidence_timeline.Services;
using MessageBox = System.Windows.MessageBox;
using WpfApp = System.Windows.Application;

namespace evidence_timeline.ViewModels
{
    public class EvidenceWindowViewModel : BaseViewModel
    {
        private readonly IEvidenceStorageService _evidenceStorage;
        private readonly IReferenceDataService _referenceData;
        private readonly CaseInfo _caseInfo;
        private readonly Evidence _sourceEvidence;
        private string _notesText = string.Empty;
        private string _loadedNotesSnapshot = string.Empty;
        private Evidence? _loadedEvidenceSnapshot;
        private CancellationTokenSource? _notesAutoSaveCts;
        private CancellationTokenSource? _metadataAutoSaveCts;
        private bool _suppressNotesAutoSave;
        private bool _suppressMetadataAutoSave;
        private readonly TimeSpan _notesAutoSaveDelay = TimeSpan.FromMilliseconds(800);
        private readonly TimeSpan _metadataAutoSaveDelay = TimeSpan.FromMilliseconds(300);

        public event EventHandler<Evidence>? EvidenceSaved;
        public event EventHandler<string>? NotesSaved;

        public EvidenceWindowViewModel(CaseInfo caseInfo, Evidence evidence, IEvidenceStorageService evidenceStorage, IReferenceDataService referenceData)
        {
            _caseInfo = caseInfo;
            _sourceEvidence = evidence;
            _evidenceStorage = evidenceStorage;
            _referenceData = referenceData;

            Evidence = CloneEvidence(evidence);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            SaveNotesCommand = new AsyncRelayCommand(SaveNotesAsync);

            TagOptions = new ObservableCollection<SelectableItem>();
            PersonOptions = new ObservableCollection<SelectableItem>();
            LinkedEvidence = new ObservableCollection<string>(Evidence.LinkedEvidenceIds);
            _loadedEvidenceSnapshot = CloneEvidence(Evidence);
        }

        public Evidence Evidence { get; }
        public ObservableCollection<EvidenceType> Types { get; } = new();
        public ObservableCollection<SelectableItem> TagOptions { get; }
        public ObservableCollection<SelectableItem> PersonOptions { get; }
        public ObservableCollection<string> LinkedEvidence { get; }

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

        public ICommand SaveCommand { get; }
        public ICommand SaveNotesCommand { get; }

        public async Task LoadAsync()
        {
            _suppressMetadataAutoSave = true;
            try
            {
                var types = await _referenceData.LoadTypesAsync(_caseInfo);
                Types.Clear();
                foreach (var type in types)
                {
                    Types.Add(type);
                }

                var tags = await _referenceData.LoadTagsAsync(_caseInfo);
                TagOptions.Clear();
                foreach (var tag in tags)
                {
                    TagOptions.Add(new SelectableItem(tag.Id, tag.Name, Evidence.TagIds.Contains(tag.Id)));
                }

                var people = await _referenceData.LoadPeopleAsync(_caseInfo);
                PersonOptions.Clear();
                foreach (var person in people)
                {
                    PersonOptions.Add(new SelectableItem(person.Id, person.Name, Evidence.PersonIds.Contains(person.Id)));
                }

                await LoadNotesAsync();
                _loadedEvidenceSnapshot = CloneEvidence(Evidence);
            }
            finally
            {
                _suppressMetadataAutoSave = false;
            }
        }

        private async Task LoadNotesAsync()
        {
            _suppressNotesAutoSave = true;
            try
            {
                var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(_caseInfo, Evidence);
                var noteFile = Evidence.NoteFile ?? "note.md";
                var notePath = System.IO.Path.Combine(folder, noteFile);
                NotesText = System.IO.File.Exists(notePath)
                    ? await System.IO.File.ReadAllTextAsync(notePath)
                    : string.Empty;
                _loadedNotesSnapshot = NotesText;
            }
            finally
            {
                _suppressNotesAutoSave = false;
            }
        }

        private async Task SaveAsync()
        {
            Evidence.TagIds = TagOptions.Where(t => t.IsSelected).Select(t => t.Id).ToList();
            Evidence.PersonIds = PersonOptions.Where(p => p.IsSelected).Select(p => p.Id).ToList();
            Evidence.LinkedEvidenceIds = LinkedEvidence.ToList();
            UpdateSortDate(Evidence);

            if (_loadedEvidenceSnapshot != null && !HasMetadataChanges(Evidence, _loadedEvidenceSnapshot))
            {
                return;
            }

            await _evidenceStorage.SaveEvidenceAsync(_caseInfo, Evidence);
            _loadedEvidenceSnapshot = CloneEvidence(Evidence);
            EvidenceSaved?.Invoke(this, CloneEvidence(Evidence));
        }

        private async Task SaveNotesAsync()
        {
            var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(_caseInfo, Evidence);
            var noteFile = Evidence.NoteFile ?? "note.md";
            var notePath = System.IO.Path.Combine(folder, noteFile);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(notePath)!);
            var currentNotes = NotesText ?? string.Empty;
            if (string.Equals(currentNotes, _loadedNotesSnapshot ?? string.Empty, StringComparison.Ordinal))
            {
                return;
            }

            await System.IO.File.WriteAllTextAsync(notePath, currentNotes);
            _loadedNotesSnapshot = currentNotes;
            NotesSaved?.Invoke(this, Evidence.Id);
        }

        public void RequestMetadataAutoSave(bool immediate = false)
        {
            if (_suppressMetadataAutoSave)
            {
                return;
            }

            if (immediate)
            {
                _ = SaveMetadataSafeAsync();
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
                    await SaveMetadataSafeAsync();
                }
                catch (OperationCanceledException)
                {
                }
            });
        }

        private void QueueNotesAutoSave()
        {
            if (_suppressNotesAutoSave)
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
                    await SaveNotesSafeAsync();
                }
                catch (OperationCanceledException)
                {
                }
            });
        }

        private async Task SaveMetadataSafeAsync()
        {
            try
            {
                if (WpfApp.Current != null)
                {
                    await WpfApp.Current.Dispatcher.InvokeAsync(async () => await SaveAsync());
                }
                else
                {
                    await SaveAsync();
                }
            }
            catch (Exception ex)
            {
                if (WpfApp.Current != null)
                {
                    await WpfApp.Current.Dispatcher.InvokeAsync(() =>
                        MessageBox.Show($"Unable to autosave metadata: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                }
            }
        }

        private async Task SaveNotesSafeAsync()
        {
            try
            {
                if (WpfApp.Current != null)
                {
                    await WpfApp.Current.Dispatcher.InvokeAsync(async () => await SaveNotesAsync());
                }
                else
                {
                    await SaveNotesAsync();
                }
            }
            catch (Exception ex)
            {
                if (WpfApp.Current != null)
                {
                    await WpfApp.Current.Dispatcher.InvokeAsync(() =>
                        MessageBox.Show($"Unable to autosave notes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                }
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
                TagIds = new List<string>(source.TagIds),
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
                return true;
            }

            if (!string.Equals(current.Title, snapshot.Title, StringComparison.Ordinal) ||
                !string.Equals(current.CourtNumber, snapshot.CourtNumber, StringComparison.Ordinal) ||
                !string.Equals(current.TypeId, snapshot.TypeId, StringComparison.Ordinal))
            {
                return true;
            }

            if (!ListMatches(current.TagIds, snapshot.TagIds) ||
                !ListMatches(current.PersonIds, snapshot.PersonIds) ||
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

        private static void UpdateSortDate(Evidence evidence)
        {
            evidence.DateInfo ??= new EvidenceDateInfo();
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
    }
}
