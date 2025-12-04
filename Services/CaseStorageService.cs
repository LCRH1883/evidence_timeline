using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using evidence_timeline.Models;
using evidence_timeline.Utilities;

namespace evidence_timeline.Services
{
    public class CaseStorageService : ICaseStorageService
    {
        public async Task<CaseInfo> CreateCaseAsync(string rootFolder, string name, string caseNumber)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var caseInfo = new CaseInfo
            {
                Name = name,
                CaseNumber = caseNumber ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                LastOpenedAt = DateTime.UtcNow,
                NextEvidenceNumber = 1
            };

            var folderName = PathHelper.GetCaseFolderName(caseInfo);
            var caseFolder = Path.Combine(rootFolder, folderName);
            PathHelper.EnsureDirectory(caseFolder);

            caseInfo.RootPath = caseFolder;

            await JsonHelper.SaveAsync(Path.Combine(caseFolder, "case.json"), caseInfo);
            await JsonHelper.SaveAsync(Path.Combine(caseFolder, "types.json"), CreateDefaultEvidenceTypes());
            await JsonHelper.SaveAsync(Path.Combine(caseFolder, "people.json"), new List<Person>());
            PathHelper.EnsureDirectory(Path.Combine(caseFolder, "evidence"));

            return caseInfo;
        }

        public async Task<CaseInfo> LoadCaseAsync(string caseFolderPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(caseFolderPath);

            var casePath = Path.Combine(caseFolderPath, "case.json");
            var caseInfo = await JsonHelper.LoadAsync<CaseInfo>(casePath);
            if (caseInfo == null)
            {
                throw new InvalidOperationException($"Unable to load case from {casePath}");
            }

            caseInfo.RootPath = caseFolderPath;
            return caseInfo;
        }

        public async Task SaveCaseAsync(CaseInfo caseInfo)
        {
            var root = GetCaseRoot(caseInfo);
            var parent = Directory.GetParent(root)?.FullName;
            if (string.IsNullOrWhiteSpace(parent))
            {
                throw new InvalidOperationException("Cannot determine parent folder for case rename.");
            }

            var desiredFolderName = PathHelper.GetCaseFolderName(caseInfo);
            var desiredPath = Path.Combine(parent, desiredFolderName);

            if (!string.Equals(root, desiredPath, StringComparison.OrdinalIgnoreCase))
            {
                if (Directory.Exists(desiredPath))
                {
                    throw new InvalidOperationException($"Cannot rename case folder to '{desiredFolderName}' because it already exists.");
                }

                Directory.Move(root, desiredPath);
                caseInfo.RootPath = desiredPath;
                root = desiredPath;
            }

            await JsonHelper.SaveAsync(Path.Combine(root, "case.json"), caseInfo);
        }

        private static string GetCaseRoot(CaseInfo caseInfo)
        {
            if (string.IsNullOrWhiteSpace(caseInfo.RootPath))
            {
                throw new InvalidOperationException("CaseInfo.RootPath must be set before saving.");
            }

            return caseInfo.RootPath;
        }

        private static List<EvidenceType> CreateDefaultEvidenceTypes()
        {
            var defaultNames = new[]
            {
                "Document(s)",
                "Contract",
                "Letter",
                "Email",
                "Message(s)",
                "Photograph",
                "Receipt",
                "Audio",
                "Video"
            };

            return defaultNames
                .Select(name => new EvidenceType
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = name
                })
                .ToList();
        }
    }
}
