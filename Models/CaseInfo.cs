using System;
using System.Text.Json.Serialization;

namespace evidence_timeline.Models
{
    public class CaseInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public string CaseNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastOpenedAt { get; set; } = DateTime.UtcNow;
        public int NextEvidenceNumber { get; set; }

        [JsonIgnore]
        public string? RootPath { get; set; }
    }
}
