using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
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
        private readonly bool _isNew;
        private bool _isCreated;
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

        public EvidenceWindowViewModel(CaseInfo caseInfo, Evidence evidence, IEvidenceStorageService evidenceStorage, IReferenceDataService referenceData, bool isNew = false)
        {
            _caseInfo = caseInfo;
            _sourceEvidence = evidence;
            _evidenceStorage = evidenceStorage;
            _referenceData = referenceData;
            _isNew = isNew;
            _isCreated = !isNew;

            Evidence = CloneEvidence(evidence);
            AttachmentItems = new ObservableCollection<AttachmentInfo>(Evidence.Attachments.Select(a => new AttachmentInfo
            {
                FileName = a.FileName,
                RelativePath = a.RelativePath
            }));
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            SaveNotesCommand = new AsyncRelayCommand(SaveNotesAsync);
            AddAttachmentCommand = new AsyncRelayCommand(AddAttachmentAsync);
            OpenAttachmentCommand = new RelayCommand<AttachmentInfo>(attachment => _ = OpenAttachmentAsync(attachment));
            OpenAttachmentFolderCommand = new RelayCommand<AttachmentInfo>(attachment => _ = OpenAttachmentFolderAsync(attachment));
            RemoveAttachmentCommand = new RelayCommand<AttachmentInfo>(attachment => _ = RemoveAttachmentAsync(attachment));

            PersonOptions = new ObservableCollection<SelectableItem>();
            LinkedEvidence = new ObservableCollection<string>(Evidence.LinkedEvidenceIds);
            _loadedEvidenceSnapshot = CloneEvidence(Evidence);

            PersonOptions.CollectionChanged += OnPersonOptionsChanged;
        }

        public Evidence Evidence { get; }
        public ObservableCollection<EvidenceType> Types { get; } = new();
        public ObservableCollection<AttachmentInfo> AttachmentItems { get; }
        public ObservableCollection<SelectableItem> PersonOptions { get; }
        public ObservableCollection<string> LinkedEvidence { get; }

        public EvidenceDateMode SelectedDateMode
        {
            get => Evidence.DateInfo?.Mode ?? EvidenceDateMode.Exact;
            set
            {
                EnsureDateInfo();
                if (Evidence.DateInfo.Mode != value)
                {
                    Evidence.DateInfo.Mode = value;
                    OnPropertyChanged();
                    RequestMetadataAutoSave();
                }
            }
        }

        public DateTime? ExactDate
        {
            get => Evidence.DateInfo?.ExactDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                EnsureDateInfo();
                Evidence.DateInfo.ExactDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
                OnPropertyChanged();
                RequestMetadataAutoSave();
            }
        }

        public DateTime? StartDate
        {
            get => Evidence.DateInfo?.StartDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                EnsureDateInfo();
                Evidence.DateInfo.StartDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
                OnPropertyChanged();
                RequestMetadataAutoSave();
            }
        }

        public DateTime? EndDate
        {
            get => Evidence.DateInfo?.EndDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                EnsureDateInfo();
                Evidence.DateInfo.EndDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
                OnPropertyChanged();
                RequestMetadataAutoSave();
            }
        }

        public int? AroundAmount
        {
            get => Evidence.DateInfo?.AroundAmount;
            set
            {
                EnsureDateInfo();
                Evidence.DateInfo.AroundAmount = value;
                OnPropertyChanged();
                RequestMetadataAutoSave();
            }
        }

        public string? AroundUnit
        {
            get => Evidence.DateInfo?.AroundUnit;
            set
            {
                EnsureDateInfo();
                Evidence.DateInfo.AroundUnit = value;
                OnPropertyChanged();
                RequestMetadataAutoSave();
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

        public ICommand SaveCommand { get; }
        public ICommand SaveNotesCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand OpenAttachmentCommand { get; }
        public ICommand OpenAttachmentFolderCommand { get; }
        public ICommand RemoveAttachmentCommand { get; }

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

                var people = await _referenceData.LoadPeopleAsync(_caseInfo);
                PersonOptions.Clear();
                foreach (var person in people)
                {
                    PersonOptions.Add(new SelectableItem(person.Id, person.Name, Evidence.PersonIds.Contains(person.Id)));
                }

                if (_isCreated)
                {
                    await LoadNotesAsync();
                    _loadedEvidenceSnapshot = CloneEvidence(Evidence);
                }
                else
                {
                    _loadedNotesSnapshot = string.Empty;
                }
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
            SyncAttachmentsToEvidence();
            Evidence.PersonIds = PersonOptions.Where(p => p.IsSelected).Select(p => p.Id).ToList();
            Evidence.LinkedEvidenceIds = LinkedEvidence.ToList();
            UpdateSortDate(Evidence);

            if (_loadedEvidenceSnapshot != null && _isCreated && !HasMetadataChanges(Evidence, _loadedEvidenceSnapshot))
            {
                return;
            }

            await EnsureCreatedAsync();
            await _evidenceStorage.SaveEvidenceAsync(_caseInfo, Evidence);
            _loadedEvidenceSnapshot = CloneEvidence(Evidence);
            EvidenceSaved?.Invoke(this, CloneEvidence(Evidence));
        }

        private async Task SaveNotesAsync()
        {
            await EnsureCreatedAsync();

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

        private void EnsureDateInfo()
        {
            Evidence.DateInfo ??= new EvidenceDateInfo();
        }

        private static void UpdateSortDate(Evidence evidence)
        {
            evidence.DateInfo ??= new EvidenceDateInfo();
            evidence.DateInfo.SortDate = ResolveSortDate(evidence.DateInfo);
        }

        private static DateOnly ResolveSortDate(EvidenceDateInfo dateInfo)
        {
            var resolvedDate = dateInfo.ExactDate
                ?? dateInfo.StartDate
                ?? dateInfo.EndDate;

            if (resolvedDate != null)
            {
                return resolvedDate.Value;
            }

            if (dateInfo.SortDate != default)
            {
                return dateInfo.SortDate;
            }

            return DateOnly.FromDateTime(DateTime.UtcNow);
        }

        private async Task AddAttachmentAsync()
        {
            using var dialog = new System.Windows.Forms.OpenFileDialog
            {
                Title = "Select attachment(s)",
                Multiselect = true
            };

            var ownerWindow = System.Windows.Application.Current?.Windows.OfType<System.Windows.Window>()
                .FirstOrDefault(w => w.IsActive) ?? System.Windows.Application.Current?.MainWindow;

            System.Windows.Forms.DialogResult result;
            if (ownerWindow != null)
            {
                var win32Owner = new Utilities.Wpf32Window(ownerWindow);
                result = dialog.ShowDialog(win32Owner);
            }
            else
            {
                result = dialog.ShowDialog();
            }

            if (result != System.Windows.Forms.DialogResult.OK || dialog.FileNames.Length == 0)
            {
                return;
            }

            await AddAttachmentsFromPathsAsync(dialog.FileNames);
        }

        public async Task AddAttachmentsFromPathsAsync(IEnumerable<string> filePaths)
        {
            var files = filePaths
                .Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (files.Count == 0)
            {
                return;
            }

            await EnsureCreatedAsync();

            var targetFolder = await _evidenceStorage.GetEvidenceFolderPathAsync(_caseInfo, Evidence);
            var filesFolder = Path.Combine(targetFolder, "files");
            Directory.CreateDirectory(filesFolder);

            var changed = false;
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                var missingExisting = AttachmentItems.FirstOrDefault(a =>
                {
                    if (string.IsNullOrWhiteSpace(a.RelativePath))
                    {
                        return false;
                    }

                    var existingPath = Path.Combine(targetFolder, a.RelativePath);
                    return string.Equals(a.FileName, fileName, StringComparison.OrdinalIgnoreCase)
                        && !File.Exists(existingPath);
                });

                if (missingExisting != null)
                {
                    var restorePath = Path.Combine(targetFolder, missingExisting.RelativePath);
                    var restoreDir = Path.GetDirectoryName(restorePath);
                    if (!string.IsNullOrWhiteSpace(restoreDir))
                    {
                        Directory.CreateDirectory(restoreDir);
                    }

                    File.Copy(file, restorePath, true);
                    missingExisting.FileName = Path.GetFileName(restorePath);
                    changed = true;
                    continue;
                }

                var targetPath = Path.Combine(filesFolder, fileName);
                targetPath = EnsureUniqueFilePath(targetPath);
                File.Copy(file, targetPath, true);

                var relative = Path.Combine("files", Path.GetFileName(targetPath));
                if (AttachmentItems.All(a => !string.Equals(a.RelativePath, relative, StringComparison.OrdinalIgnoreCase)))
                {
                    AttachmentItems.Add(new AttachmentInfo
                    {
                        FileName = Path.GetFileName(targetPath),
                        RelativePath = relative
                    });
                    changed = true;
                }
            }

            if (changed)
            {
                await SaveAsync();
            }
        }

        private async Task OpenAttachmentAsync(AttachmentInfo? attachment)
        {
            if (attachment == null)
            {
                return;
            }

            var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(_caseInfo, Evidence);
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
            if (attachment == null)
            {
                return;
            }

            var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(_caseInfo, Evidence);
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
            var target = attachment ?? AttachmentItems.LastOrDefault();
            if (target == null)
            {
                return;
            }

            var folder = await _evidenceStorage.GetEvidenceFolderPathAsync(_caseInfo, Evidence);
            var fullPath = Path.Combine(folder, target.RelativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            AttachmentItems.Remove(target);
            await SaveAsync();
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
                candidate = Path.Combine(directory, $"{name} ({counter}){extension}");
                counter++;
            }
            while (File.Exists(candidate));

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

        private async Task EnsureCreatedAsync()
        {
            if (_isCreated)
            {
                return;
            }

            var created = await _evidenceStorage.CreateEvidenceAsync(_caseInfo, Evidence);
            Evidence.Id = created.Id;
            Evidence.EvidenceNumber = created.EvidenceNumber;
            Evidence.CreatedAt = created.CreatedAt;
            Evidence.LastModifiedAt = created.LastModifiedAt;
            Evidence.NoteFile = string.IsNullOrWhiteSpace(created.NoteFile) ? Evidence.NoteFile : created.NoteFile;
            _isCreated = true;
            _loadedEvidenceSnapshot = CloneEvidence(Evidence);
        }

        private void SyncAttachmentsToEvidence()
        {
            Evidence.Attachments = AttachmentItems.ToList();
        }
    }
}
