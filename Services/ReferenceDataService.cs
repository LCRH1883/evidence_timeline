using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using evidence_timeline.Models;
using evidence_timeline.Utilities;

namespace evidence_timeline.Services
{
    public class ReferenceDataService : IReferenceDataService
    {
        public async Task<List<Tag>> LoadTagsAsync(CaseInfo caseInfo)
        {
            var path = GetCaseFilePath(caseInfo, "tags.json");
            var tags = await JsonHelper.LoadAsync<List<Tag>>(path);
            return tags ?? new List<Tag>();
        }

        public async Task SaveTagsAsync(CaseInfo caseInfo, List<Tag> tags)
        {
            var path = GetCaseFilePath(caseInfo, "tags.json");
            await JsonHelper.SaveAsync(path, tags);
        }

        public async Task<List<EvidenceType>> LoadTypesAsync(CaseInfo caseInfo)
        {
            var path = GetCaseFilePath(caseInfo, "types.json");
            var types = await JsonHelper.LoadAsync<List<EvidenceType>>(path);
            return types ?? new List<EvidenceType>();
        }

        public async Task SaveTypesAsync(CaseInfo caseInfo, List<EvidenceType> types)
        {
            var path = GetCaseFilePath(caseInfo, "types.json");
            await JsonHelper.SaveAsync(path, types);
        }

        public async Task<List<Person>> LoadPeopleAsync(CaseInfo caseInfo)
        {
            var path = GetCaseFilePath(caseInfo, "people.json");
            var people = await JsonHelper.LoadAsync<List<Person>>(path);
            return people ?? new List<Person>();
        }

        public async Task SavePeopleAsync(CaseInfo caseInfo, List<Person> people)
        {
            var path = GetCaseFilePath(caseInfo, "people.json");
            await JsonHelper.SaveAsync(path, people);
        }

        private static string GetCaseFilePath(CaseInfo caseInfo, string fileName)
        {
            var root = caseInfo.RootPath;
            if (string.IsNullOrWhiteSpace(root))
            {
                throw new InvalidOperationException("CaseInfo.RootPath must be set before accessing reference data.");
            }

            return Path.Combine(root, fileName);
        }
    }
}
