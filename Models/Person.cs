using System.Collections.Generic;

namespace evidence_timeline.Models
{
    public class Person
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> Aliases { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
    }
}
