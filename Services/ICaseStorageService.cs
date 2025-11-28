using System.Threading.Tasks;
using evidence_timeline.Models;

namespace evidence_timeline.Services
{
    public interface ICaseStorageService
    {
        Task<CaseInfo> CreateCaseAsync(string rootFolder, string name, string caseNumber);
        Task<CaseInfo> LoadCaseAsync(string caseFolderPath);
        Task SaveCaseAsync(CaseInfo caseInfo);
    }
}
