using System.Collections.Generic;
using System.Threading.Tasks;
using evidence_timeline.Models;

namespace evidence_timeline.Services
{
    public interface IReferenceDataService
    {
        Task<List<EvidenceType>> LoadTypesAsync(CaseInfo caseInfo);
        Task SaveTypesAsync(CaseInfo caseInfo, List<EvidenceType> types);

        Task<List<Person>> LoadPeopleAsync(CaseInfo caseInfo);
        Task SavePeopleAsync(CaseInfo caseInfo, List<Person> people);
    }
}
