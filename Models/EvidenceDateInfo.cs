using System;

namespace evidence_timeline.Models
{
    public enum EvidenceDateMode
    {
        Exact,
        Around,
        Broad
    }

    public class EvidenceDateInfo
    {
        public EvidenceDateMode Mode { get; set; }
        public DateOnly? ExactDate { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? AroundAmount { get; set; }
        public string? AroundUnit { get; set; }
        public DateOnly SortDate { get; set; }
    }
}
