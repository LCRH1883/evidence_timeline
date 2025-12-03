using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using evidence_timeline.Models;
using evidence_timeline.Utilities;

namespace evidence_timeline.Services
{
    public class EvidenceStorageService : IEvidenceStorageService
    {
        public async Task<List<Evidence>> LoadAllEvidenceAsync(CaseInfo caseInfo)
        {
            var evidenceRoot = GetEvidenceRoot(caseInfo);
            var evidenceList = new List<Evidence>();

            if (!Directory.Exists(evidenceRoot))
            {
                return evidenceList;
            }

            foreach (var folder in Directory.EnumerateDirectories(evidenceRoot))
            {
                var evidencePath = Path.Combine(folder, "evidence.json");
                if (!File.Exists(evidencePath))
                {
                    continue;
                }

                var evidence = await JsonHelper.LoadAsync<Evidence>(evidencePath);
                if (evidence == null)
                {
                    continue;
                }

                EnsureDateInfo(evidence);
                EnsureEvidenceLists(evidence);
                evidenceList.Add(evidence);
            }

            return evidenceList;
        }

        public async Task<Evidence> CreateEvidenceAsync(CaseInfo caseInfo, Evidence newEvidence)
        {
            ArgumentNullException.ThrowIfNull(newEvidence);

            var evidenceRoot = GetEvidenceRoot(caseInfo);
            PathHelper.EnsureDirectory(evidenceRoot);

            if (newEvidence.EvidenceNumber <= 0)
            {
                newEvidence.EvidenceNumber = Math.Max(caseInfo.NextEvidenceNumber, 1);
            }

            if (string.IsNullOrWhiteSpace(newEvidence.Id))
            {
                newEvidence.Id = $"evidence-{newEvidence.EvidenceNumber:D4}";
            }

            EnsureDateInfo(newEvidence);
            EnsureEvidenceLists(newEvidence);
            newEvidence.CreatedAt = DateTime.UtcNow;
            newEvidence.LastModifiedAt = newEvidence.CreatedAt;
            newEvidence.NoteFile = string.IsNullOrWhiteSpace(newEvidence.NoteFile) ? "note.md" : newEvidence.NoteFile;

            var folderPath = await GetEvidenceFolderPathInternalAsync(caseInfo, newEvidence, false);
            PathHelper.EnsureDirectory(folderPath);
            PathHelper.EnsureDirectory(Path.Combine(folderPath, "files"));

            var notePath = Path.Combine(folderPath, newEvidence.NoteFile);
            if (!File.Exists(notePath))
            {
                await File.WriteAllTextAsync(notePath, string.Empty);
            }

            await SaveEvidenceFileAsync(folderPath, newEvidence);

            caseInfo.NextEvidenceNumber = newEvidence.EvidenceNumber + 1;
            await JsonHelper.SaveAsync(Path.Combine(GetCaseRoot(caseInfo), "case.json"), caseInfo);

            return newEvidence;
        }

        public async Task SaveEvidenceAsync(CaseInfo caseInfo, Evidence evidence)
        {
            ArgumentNullException.ThrowIfNull(evidence);
            EnsureDateInfo(evidence);
            EnsureEvidenceLists(evidence);
            evidence.LastModifiedAt = DateTime.UtcNow;
            evidence.NoteFile = string.IsNullOrWhiteSpace(evidence.NoteFile) ? "note.md" : evidence.NoteFile;

            var folderPath = await GetEvidenceFolderPathInternalAsync(caseInfo, evidence, true);
            PathHelper.EnsureDirectory(folderPath);
            await SaveEvidenceFileAsync(folderPath, evidence);
        }

        public async Task DeleteEvidenceAsync(CaseInfo caseInfo, string evidenceId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(evidenceId);

            var evidenceRoot = GetEvidenceRoot(caseInfo);
            var existingFolder = await FindEvidenceFolderByIdAsync(evidenceRoot, evidenceId);
            if (!string.IsNullOrWhiteSpace(existingFolder) && Directory.Exists(existingFolder))
            {
                PrepareDirectoryForDelete(existingFolder);

                try
                {
                    Directory.Delete(existingFolder, true);
                }
                catch (UnauthorizedAccessException)
                {
                    // If deletion fails due to attributes/locks, try one more time after clearing attributes.
                    PrepareDirectoryForDelete(existingFolder);
                    Directory.Delete(existingFolder, true);
                }
            }
        }

        public Task<string> GetEvidenceFolderPathAsync(CaseInfo caseInfo, Evidence evidence)
        {
            ArgumentNullException.ThrowIfNull(evidence);
            EnsureDateInfo(evidence);
            return GetEvidenceFolderPathInternalAsync(caseInfo, evidence, true);
        }

        private static async Task<string> GetEvidenceFolderPathInternalAsync(CaseInfo caseInfo, Evidence evidence, bool preferExisting)
        {
            var evidenceRoot = GetEvidenceRoot(caseInfo);

            if (preferExisting)
            {
                var existing = await FindEvidenceFolderByIdAsync(evidenceRoot, evidence.Id);
                if (!string.IsNullOrWhiteSpace(existing))
                {
                    return existing;
                }
            }

            var folderName = PathHelper.GetEvidenceFolderName(
                evidence.EvidenceNumber,
                evidence.DateInfo.SortDate,
                evidence.Title,
                evidence.TypeId);

            return Path.Combine(evidenceRoot, folderName);
        }

        private static async Task<string?> FindEvidenceFolderByIdAsync(string evidenceRoot, string evidenceId)
        {
            if (string.IsNullOrWhiteSpace(evidenceId) || !Directory.Exists(evidenceRoot))
            {
                return null;
            }

            foreach (var folder in Directory.EnumerateDirectories(evidenceRoot))
            {
                var evidencePath = Path.Combine(folder, "evidence.json");
                if (!File.Exists(evidencePath))
                {
                    continue;
                }

                var existing = await JsonHelper.LoadAsync<Evidence>(evidencePath);
                if (existing != null && string.Equals(existing.Id, evidenceId, StringComparison.OrdinalIgnoreCase))
                {
                    return folder;
                }
            }

            return null;
        }

        private static void PrepareDirectoryForDelete(string path)
        {
            // Clear read-only/hidden attributes that can block deletion (e.g., OneDrive/sync paths).
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                }
                catch
                {
                    // Ignore and let delete attempt surface any remaining errors.
                }
            }

            foreach (var dir in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.SetAttributes(dir, FileAttributes.Normal);
                }
                catch
                {
                    // Best effort.
                }
            }

            try
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }
            catch
            {
                // Best effort on the root directory.
            }
        }

        private static async Task SaveEvidenceFileAsync(string folderPath, Evidence evidence)
        {
            var evidencePath = Path.Combine(folderPath, "evidence.json");
            await JsonHelper.SaveAsync(evidencePath, evidence);
        }

        private static void EnsureEvidenceLists(Evidence evidence)
        {
            evidence.PersonIds ??= new List<string>();
            evidence.LinkedEvidenceIds ??= new List<string>();
            evidence.Attachments ??= new List<AttachmentInfo>();
        }

        private static void EnsureDateInfo(Evidence evidence)
        {
            evidence.DateInfo ??= new EvidenceDateInfo();

            if (evidence.DateInfo.SortDate == default)
            {
                evidence.DateInfo.SortDate = ResolveSortDate(evidence.DateInfo);
            }
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

        private static string GetEvidenceRoot(CaseInfo caseInfo)
        {
            var root = GetCaseRoot(caseInfo);
            return Path.Combine(root, "evidence");
        }

        private static string GetCaseRoot(CaseInfo caseInfo)
        {
            if (string.IsNullOrWhiteSpace(caseInfo.RootPath))
            {
                throw new InvalidOperationException("CaseInfo.RootPath must be set before accessing evidence storage.");
            }

            return caseInfo.RootPath;
        }
    }
}
