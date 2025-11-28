using System.Collections.Generic;
using System.Threading.Tasks;
using evidence_timeline.Models;

namespace evidence_timeline.Services
{
    public interface IEvidenceStorageService
    {
        Task<List<Evidence>> LoadAllEvidenceAsync(CaseInfo caseInfo);
        Task<Evidence> CreateEvidenceAsync(CaseInfo caseInfo, Evidence newEvidence);
        Task SaveEvidenceAsync(CaseInfo caseInfo, Evidence evidence);
        Task DeleteEvidenceAsync(CaseInfo caseInfo, string evidenceId);
        Task<string> GetEvidenceFolderPathAsync(CaseInfo caseInfo, Evidence evidence);
    }
}
