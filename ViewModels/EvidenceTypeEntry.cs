using System;
using evidence_timeline.Models;

namespace evidence_timeline.ViewModels
{
    public class EvidenceTypeEntry : BaseViewModel
    {
        private string _name = string.Empty;
        private string? _description;

        public EvidenceTypeEntry(string id, string name, string? description = null)
        {
            Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
            _name = name ?? string.Empty;
            _description = description;
        }

        public string Id { get; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, (value ?? string.Empty).Trim());
        }

        public string? Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public EvidenceType ToEvidenceType()
        {
            return new EvidenceType
            {
                Id = Id,
                Name = Name ?? string.Empty,
                Description = Description
            };
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? base.ToString() ?? nameof(EvidenceTypeEntry) : Name;
        }
    }
}
