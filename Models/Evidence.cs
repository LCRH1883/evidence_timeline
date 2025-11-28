using System;
using System.Collections.Generic;

namespace evidence_timeline.Models
{
    public class Evidence
    {
        public string Id { get; set; } = string.Empty;
        public int EvidenceNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CourtNumber { get; set; } = string.Empty;
        public EvidenceDateInfo DateInfo { get; set; } = new();
        public string TypeId { get; set; } = string.Empty;
        public List<string> TagIds { get; set; } = new();
        public List<string> PersonIds { get; set; } = new();
        public List<string> LinkedEvidenceIds { get; set; } = new();
        public string NoteFile { get; set; } = "note.md";
        public List<AttachmentInfo> Attachments { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    }
}
