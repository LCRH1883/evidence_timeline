using System;

namespace evidence_timeline.Models
{
    public class EvidenceSummary
    {
        public string Id { get; set; } = string.Empty;
        public int EvidenceNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime SortDate { get; set; }
        public string DateDisplay { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string[] TagNames { get; set; } = Array.Empty<string>();
        public string[] PersonNames { get; set; } = Array.Empty<string>();
        public string CourtNumber { get; set; } = string.Empty;
        public string SearchKey { get; set; } = string.Empty;

        public string TagNamesDisplay => string.Join(", ", TagNames ?? Array.Empty<string>());
        public string PersonNamesDisplay => string.Join(", ", PersonNames ?? Array.Empty<string>());
    }
}
