using System.Collections.Generic;

namespace evidence_timeline.Models
{
    public class AppSettings
    {
        public List<string> RecentCases { get; set; } = new();
        public string? Theme { get; set; }
    }
}
